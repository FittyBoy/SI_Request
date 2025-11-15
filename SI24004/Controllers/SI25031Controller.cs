using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SI24004.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SI25031Controller : ControllerBase
    {
        private readonly PostgrestContext _context;
        private readonly sqlServerContext _sqlServerContext;

        public SI25031Controller(PostgrestContext context, sqlServerContext sqlServerContext)
        {
            _context = context;
            _sqlServerContext = sqlServerContext;
        }

        /// <summary>
        /// ค้นหา LOT จาก ThRecord โดยใช้ ImobileLot (ไม่บันทึกทันที รอ confirm จาก Modal)
        /// Logic:
        /// 1. ค้นหาจาก ThRecord โดยใช้ ImobileLot
        /// 2. ถ้า status = "rescreen" ให้เช็คใน Th100Record
        /// 3. ส่งข้อมูลกลับไปให้ Frontend แสดงใน Modal
        /// </summary>
        [HttpPost("search-lot")]
        public async Task<IActionResult> SearchLot([FromBody] SearchLotRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.LotNumber))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "กรุณาระบุ LOT Number"
                    });
                }

                // ค้นหาจาก ThRecord โดยใช้ ImobileLot
                var thRecord = await _sqlServerContext.ThRecords
                    .FirstOrDefaultAsync(t => t.ImobileLot == request.LotNumber);

                if (thRecord == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"ไม่พบข้อมูล ImobileLot: {request.LotNumber}"
                    });
                }

                bool checkSt = false;
                bool hasTH100 = false;
                string finalStatus = thRecord.Status;
                string th100Status = null;

                // ตรวจสอบ Logic
                if (thRecord.Status?.ToLower() == "rescreen")
                {
                    var th100Record = await _sqlServerContext.Th100Records
                        .FirstOrDefaultAsync(t => t.LotPo == thRecord.LotPo || t.McPo == thRecord.McPo || t.NoPo == thRecord.NoPo);

                    if (th100Record != null)
                    {
                        hasTH100 = true;
                        th100Status = th100Record.Status;

                        if (th100Record.Status?.ToUpper() == "OK")
                        {
                            checkSt = true;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(thRecord.Status))
                {
                    // Status อื่นๆ ที่ไม่ใช่ rescreen
                    checkSt = thRecord.Status.ToUpper() == "OK";
                }

                // สร้าง PO LOT จาก LotPo + McPo + NoPo
                string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";

                // ตรวจสอบว่ามี LOT นี้ในระบบแล้วหรือไม่
                var existingRecord = await _context.PoCheckFlows
                    .FirstOrDefaultAsync(p =>
                        p.Imobilelot == thRecord.ImobileLot &&
                        p.CheckDate == DateTime.UtcNow.Date);

                return Ok(new
                {
                    success = true,
                    message = "พบข้อมูล LOT",
                    data = new
                    {
                        imobileLot = thRecord.ImobileLot,
                        poLot = poLot,
                        statusTn = finalStatus,
                        checkSt = checkSt,
                        mcNo = thRecord.McPo,
                        hasTH100 = hasTH100,
                        th100Status = th100Status,
                        isDuplicate = existingRecord != null,
                        existingQty = existingRecord?.LotQty
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการค้นหา",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// บันทึก LOT พร้อม Qty หลังจาก confirm จาก Modal
        /// </summary>
        [HttpPost("save-lot")]
        public async Task<IActionResult> SaveLot([FromBody] SaveLotRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ImobileLot))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "กรุณาระบุ ImobileLot"
                    });
                }

                // ค้นหาข้อมูลจาก ThRecord อีกครั้งเพื่อความแน่ใจ
                var thRecord = await _sqlServerContext.ThRecords
                    .FirstOrDefaultAsync(t => t.ImobileLot == request.ImobileLot);

                if (thRecord == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "ไม่พบข้อมูล LOT"
                    });
                }

                bool checkSt = false;
                string finalStatus = thRecord.Status;

                // ตรวจสอบ Logic อีกครั้ง
                if (thRecord.Status?.ToLower() == "rescreen")
                {
                    var th100Record = await _sqlServerContext.Th100Records
                        .FirstOrDefaultAsync(t => t.LotPo == thRecord.LotPo || t.McPo == thRecord.McPo || t.NoPo == thRecord.NoPo);

                    if (th100Record != null && th100Record.Status?.ToUpper() == "OK")
                    {
                        checkSt = true;
                        finalStatus = "OK (Rescreen)";
                    }
                    else if (th100Record != null)
                    {
                        checkSt = false;
                        finalStatus = th100Record.Status ?? "Rescreen";
                    }
                    else
                    {
                        // ไม่พบใน Th100Record - ไม่ควรบันทึก
                        return BadRequest(new
                        {
                            success = false,
                            message = "LOT นี้ Rescreen Pending ไม่สามารถบันทึกได้"
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(thRecord.Status))
                {
                    checkSt = thRecord.Status.ToUpper() == "OK";

                    // ตรวจสอบว่าเป็น Hold หรือ Scrap
                    if (thRecord.Status.ToLower() == "hold" || thRecord.Status.ToLower() == "scrap")
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"LOT นี้อยู่ในสถานะ {thRecord.Status} ไม่สามารถบันทึกได้"
                        });
                    }
                }

                // สร้าง PO LOT
                string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";

                // รับ Qty จาก Request
                int totalQty = request.LotQty ?? 0;

                if (totalQty <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "กรุณาระบุจำนวน Lot Qty"
                    });
                }

                // ตรวจสอบว่ามีข้อมูลซ้ำหรือไม่
                var existingRecord = await _context.PoCheckFlows
                    .FirstOrDefaultAsync(p =>
                        p.Imobilelot == thRecord.ImobileLot &&
                        p.CheckDate == DateTime.UtcNow.Date);

                PoCheckFlow poCheckFlow;

                if (existingRecord != null)
                {
                    // อัพเดทข้อมูลเดิม
                    existingRecord.StatusTn = finalStatus;
                    existingRecord.CheckSt = checkSt;
                    existingRecord.CheckDate = DateTime.UtcNow.Date;
                    existingRecord.McNo = thRecord.McPo;
                    existingRecord.PoLot = poLot;
                    existingRecord.LotQty = totalQty;

                    poCheckFlow = existingRecord;
                }
                else
                {
                    // บันทึกข้อมูลใหม่
                    poCheckFlow = new PoCheckFlow
                    {
                        PoLot = poLot,
                        Imobilelot = thRecord.ImobileLot,
                        StatusTn = finalStatus,
                        CheckSt = checkSt,
                        CheckDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
                        McNo = thRecord.McPo,
                        LotQty = totalQty
                    };

                    _context.PoCheckFlows.Add(poCheckFlow);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"บันทึก LOT {poLot} สำเร็จ",
                    data = new
                    {
                        id = poCheckFlow.Id,
                        poLot = poCheckFlow.PoLot,
                        imobileLot = poCheckFlow.Imobilelot,
                        statusTn = poCheckFlow.StatusTn,
                        checkSt = poCheckFlow.CheckSt,
                        checkDate = poCheckFlow.CheckDate,
                        mcNo = poCheckFlow.McNo,
                        lotQty = poCheckFlow.LotQty
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการบันทึก",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// ดึงข้อมูล LOT ตาม MC และวันที่
        /// </summary>
        [HttpGet("get-lots-by-mc")]
        public async Task<IActionResult> GetLotsByMc([FromQuery] string mcNo, [FromQuery] DateTime? date)
        {
            try
            {
                var query = _context.PoCheckFlows.AsQueryable();

                if (!string.IsNullOrEmpty(mcNo))
                {
                    query = query.Where(p => p.McNo == mcNo);
                }

                if (date.HasValue)
                {
                    var targetDate = date.Value.Date;
                    query = query.Where(p => p.CheckDate == targetDate);
                }
                else
                {
                    // ถ้าไม่ระบุวันที่ ให้ดึงวันนี้
                    var today = DateTime.UtcNow.Date;
                    query = query.Where(p => p.CheckDate == today);
                }

                var records = await query
                    .OrderBy(p => p.CheckDate)
                    .ThenBy(p => p.Id)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = records.Select(r => new
                    {
                        id = r.Id,
                        poLot = r.PoLot,
                        imobileLot = r.Imobilelot,
                        statusTn = r.StatusTn,
                        checkSt = r.CheckSt,
                        check = r.CheckSt ? "OK" : "NG",
                        checkDate = r.CheckDate,
                        mcNo = r.McNo,
                        lotQty = r.LotQty
                    }).ToList(),
                    totalCount = records.Count,
                    okCount = records.Count(r => r.CheckSt),
                    ngCount = records.Count(r => !r.CheckSt)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูล",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ลบ LOT (สำหรับกรณีที่ต้องการแก้ไข)
        /// </summary>
        [HttpDelete("delete-lot/{id}")]
        public async Task<IActionResult> DeleteLot(Guid id)
        {
            try
            {
                var record = await _context.PoCheckFlows.FindAsync(id);

                if (record == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "ไม่พบข้อมูล LOT"
                    });
                }

                _context.PoCheckFlows.Remove(record);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "ลบข้อมูล LOT สำเร็จ"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการลบข้อมูล",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ดึงสถิติรายวัน
        /// </summary>
        [HttpGet("get-daily-stats")]
        public async Task<IActionResult> GetDailyStats([FromQuery] string mcNo, [FromQuery] DateTime? date)
        {
            try
            {
                var targetDate = date?.Date ?? DateTime.UtcNow.Date;
                var query = _context.PoCheckFlows.AsQueryable();

                if (!string.IsNullOrEmpty(mcNo))
                {
                    query = query.Where(p => p.McNo == mcNo);
                }

                query = query.Where(p => p.CheckDate == targetDate);

                var records = await query.ToListAsync();
                var totalQty = records.Sum(r => r.LotQty ?? 0);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        date = targetDate,
                        mcNo = mcNo,
                        totalLots = records.Count,
                        okCount = records.Count(r => r.CheckSt),
                        ngCount = records.Count(r => !r.CheckSt),
                        totalQty = totalQty,
                        canAddMore = records.Count < 8
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงสถิติ",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ตรวจสอบและบันทึกข้อมูลทั้งหมด (Batch Processing)
        /// </summary>
        [HttpPost("check-and-save")]
        public async Task<IActionResult> CheckAndSaveAll()
        {
            try
            {
                var savedRecords = new List<object>();
                var thRecords = await _sqlServerContext.ThRecords.ToListAsync();

                foreach (var thRecord in thRecords)
                {
                    bool shouldSave = false;
                    bool checkSt = false;
                    string finalStatus = thRecord.Status;

                    if (thRecord.Status?.ToLower() == "rescreen")
                    {
                        var th100Record = await _sqlServerContext.Th100Records
                            .FirstOrDefaultAsync(t => t.ImobileLot == thRecord.ImobileLot);

                        if (th100Record != null && th100Record.Status?.ToUpper() == "OK")
                        {
                            shouldSave = true;
                            checkSt = true;
                            finalStatus = "OK (Rescreen)";
                        }
                    }
                    else if (!string.IsNullOrEmpty(thRecord.Status))
                    {
                        // ข้าม Hold และ Scrap
                        if (thRecord.Status.ToLower() != "hold" && thRecord.Status.ToLower() != "scrap")
                        {
                            shouldSave = true;
                            checkSt = thRecord.Status.ToUpper() == "OK";
                        }
                    }

                    if (shouldSave)
                    {
                        string poLot = $"{thRecord.LotPo}{thRecord.McPo}{thRecord.NoPo}";

                        var poCheckFlow = new PoCheckFlow
                        {
                            PoLot = poLot,
                            Imobilelot = thRecord.ImobileLot,
                            StatusTn = finalStatus,
                            CheckSt = checkSt,
                            CheckDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
                            McNo = thRecord.McPo,
                            LotQty = 0 // Default value for batch processing
                        };

                        _context.PoCheckFlows.Add(poCheckFlow);
                        savedRecords.Add(new
                        {
                            poLot = poCheckFlow.PoLot,
                            imobileLot = poCheckFlow.Imobilelot,
                            statusTn = poCheckFlow.StatusTn,
                            checkSt = poCheckFlow.CheckSt,
                            mcNo = poCheckFlow.McNo
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"บันทึกข้อมูลสำเร็จ {savedRecords.Count} รายการ",
                    data = savedRecords
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการบันทึกข้อมูล",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ตรวจสอบและบันทึกข้อมูลสำหรับ MC และวันที่ที่ระบุ
        /// </summary>
        [HttpPost("check-and-save-by-mc")]
        public async Task<IActionResult> CheckAndSaveByMcAndDate([FromBody] CheckFlowRequest request)
        {
            try
            {
                var savedRecords = new List<object>();
                var query = _sqlServerContext.ThRecords.AsQueryable();

                if (!string.IsNullOrEmpty(request.McPo))
                {
                    query = query.Where(t => t.McPo == request.McPo);
                }

                if (request.DateProcess.HasValue)
                {
                    var targetDate = request.DateProcess.Value.Date;
                    query = query.Where(t =>
                        t.DateProcess.Year == targetDate.Year &&
                        t.DateProcess.Month == targetDate.Month &&
                        t.DateProcess.Day == targetDate.Day);
                }

                var thRecords = await query.ToListAsync();

                foreach (var thRecord in thRecords)
                {
                    bool shouldSave = false;
                    bool checkSt = false;
                    string finalStatus = thRecord.Status;

                    if (thRecord.Status?.ToLower() == "rescreen")
                    {
                        var th100Record = await _sqlServerContext.Th100Records
                            .FirstOrDefaultAsync(t => t.ImobileLot == thRecord.ImobileLot);

                        if (th100Record != null && th100Record.Status?.ToUpper() == "OK")
                        {
                            shouldSave = true;
                            checkSt = true;
                            finalStatus = "OK (Rescreen)";
                        }
                    }
                    else if (!string.IsNullOrEmpty(thRecord.Status))
                    {
                        if (thRecord.Status.ToLower() != "hold" && thRecord.Status.ToLower() != "scrap")
                        {
                            shouldSave = true;
                            checkSt = thRecord.Status.ToUpper() == "OK";
                        }
                    }

                    if (shouldSave)
                    {
                        string poLot = $"{thRecord.LotPo}{thRecord.McPo}{thRecord.NoPo}";

                        var poCheckFlow = new PoCheckFlow
                        {
                            PoLot = poLot,
                            Imobilelot = thRecord.ImobileLot,
                            StatusTn = finalStatus,
                            CheckSt = checkSt,
                            CheckDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
                            McNo = thRecord.McPo,
                            LotQty = 0
                        };

                        _context.PoCheckFlows.Add(poCheckFlow);
                        savedRecords.Add(new
                        {
                            poLot = poCheckFlow.PoLot,
                            imobileLot = poCheckFlow.Imobilelot,
                            statusTn = poCheckFlow.StatusTn,
                            checkSt = poCheckFlow.CheckSt,
                            mcNo = poCheckFlow.McNo
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"บันทึกข้อมูลสำเร็จ {savedRecords.Count} รายการ",
                    data = savedRecords
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการบันทึกข้อมูล",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ดึงข้อมูล Flow-out Check ทั้งหมด
        /// </summary>
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllFlowOutChecks()
        {
            try
            {
                var records = await _context.PoCheckFlows
                    .OrderByDescending(p => p.CheckDate)
                    .ThenBy(p => p.Id)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = records
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูล",
                    error = ex.Message
                });
            }
        }
    }

    // Request Models
    public class SearchLotRequest
    {
        public string? LotNumber { get; set; }
    }

    public class SaveLotRequest
    {
        public string? ImobileLot { get; set; }
        public int? LotQty { get; set; }
    }

    public class CheckFlowRequest
    {
        public string? McPo { get; set; }
        public DateTime? DateProcess { get; set; }
    }
}