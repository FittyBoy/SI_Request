using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SI24004.Models;
using SI24004.ModelsSQL;
using SI24004.ModelsSqlServer1;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SI25031Controller : ControllerBase
    {
        private readonly PostgrestContext _context;
        private readonly ThicknessContext _thicknessContext;
        private readonly OutputContext _outputContext;

        public SI25031Controller(PostgrestContext context, ThicknessContext thicknessContext, OutputContext outputContext)
        {
            _context = context;
            _thicknessContext = thicknessContext;
            _outputContext = outputContext;
        }

        /// <summary>
        /// ค้นหา LOT จาก ThRecord โดยใช้ ImobileLot (ไม่บันทึกทันที รอ confirm จาก Modal)
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
                var thRecord = await _thicknessContext.ThRecords
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
                    var th100Record = await _thicknessContext.Th100Records
                        .FirstOrDefaultAsync(t =>
                            t.ImobileLot == thRecord.ImobileLot ||
                            (t.LotPo == thRecord.LotPo && t.McPo == thRecord.McPo && t.NoPo == thRecord.NoPo));

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
                    checkSt = thRecord.Status.ToUpper() == "OK";
                }

                // สร้าง PO LOT
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

                var thRecord = await _thicknessContext.ThRecords
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
                    var th100Record = await _thicknessContext.Th100Records
                        .FirstOrDefaultAsync(t =>
                            t.ImobileLot == thRecord.ImobileLot ||
                            (t.LotPo == thRecord.LotPo && t.McPo == thRecord.McPo && t.NoPo == thRecord.NoPo));

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

                    if (thRecord.Status.ToLower() == "hold" || thRecord.Status.ToLower() == "scrap")
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"LOT นี้อยู่ในสถานะ {thRecord.Status} ไม่สามารถบันทึกได้"
                        });
                    }
                }

                string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";
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
                        check = poCheckFlow.CheckSt ? "OK" : "NG",
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
        /// ดึงข้อมูล LOT ตาม MC (แสดงเฉพาะ 8 LOT ล่าสุดต่อ MC) เรียงตามเลข 3 ตัวท้าย
        /// </summary>
        [HttpGet("get-lots-by-mc")]
        public async Task<IActionResult> GetLotsByMc([FromQuery] string mcNo)
        {
            try
            {
                if (string.IsNullOrEmpty(mcNo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "กรุณาระบุ MC Number"
                    });
                }

                // ดึงข้อมูลของ MC ที่เลือก วันนี้เท่านั้น
                var today = DateTime.UtcNow.Date;
                var records = await _context.PoCheckFlows
                    .Where(p => p.McNo == mcNo && p.CheckDate == today)
                    .ToListAsync();

                // นับจำนวน LOT ทั้งหมดของ MC นี้ในวันนี้
                int totalCount = records.Count;
                int okCount = records.Count(r => r.CheckSt);
                int ngCount = records.Count(r => !r.CheckSt);

                // เรียงลำดับตาม LOT โดยแยกเลข 3 ตัวท้ายและตัวอักษร
                var sortedRecords = records
                    .Select(r => new
                    {
                        Record = r,
                        Parts = r.PoLot?.Split('-') ?? new string[0],
                        LastPart = r.PoLot?.Split('-').LastOrDefault() ?? "",
                    })
                    .Select(x => new
                    {
                        x.Record,
                        x.Parts,
                        x.LastPart,
                        Number = int.TryParse(System.Text.RegularExpressions.Regex.Match(x.LastPart, @"^\d+").Value, out int num) ? num : 0,
                        Letter = System.Text.RegularExpressions.Regex.Match(x.LastPart, @"[A-Za-z]+$").Value
                    })
                    .OrderByDescending(x => x.Number)  // เรียงจากมากไปน้อย (ล่าสุดก่อน)
                    .ThenByDescending(x => x.Letter)   // ถ้าเลขเท่ากัน เรียงตามตัวอักษร
                    .Take(8)                           // เอาเฉพาะ 8 LOT ล่าสุดของ MC นี้
                    .Select(x => x.Record)
                    .Reverse()                         // กลับลำดับเพื่อแสดง LOT เก่าก่อน LOT ใหม่
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = sortedRecords.Select(r => new
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
                    totalCount = totalCount,           // จำนวน LOT ทั้งหมดของ MC นี้ในวันนี้
                    displayCount = sortedRecords.Count, // จำนวนที่แสดง (สูงสุด 8)
                    okCount = okCount,                 // OK ทั้งหมดของ MC นี้
                    ngCount = ngCount                  // NG ทั้งหมดของ MC นี้
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
        /// ดึงรายการ MC ทั้งหมดจาก PoCheckFlows
        /// </summary>
        [HttpGet("get-mc-list")]
        public async Task<IActionResult> GetMcList()
        {
            try
            {
                // ดึง MC ที่มีใน PoCheckFlows
                var mcListInCleaning = await _context.PoCheckFlows
                    .Where(t => !string.IsNullOrEmpty(t.McNo))
                    .Select(t => t.McNo)
                    .Distinct()
                    .ToListAsync();

                // ดึง MC จาก ThRecords
                var mcList = await _thicknessContext.ThRecords
                    .Where(t => !string.IsNullOrEmpty(t.McPo))
                    .Select(t => t.McPo)
                    .Distinct()
                    .ToListAsync();

                // เอาเฉพาะ MC ที่มีใน mcListInCleaning
                var filteredMcList = mcList
                    .Where(mc => mcListInCleaning.Contains(mc))
                    .OrderBy(mc => mc)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = filteredMcList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงรายการ MC",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ลบ LOT
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
                        totalQty = totalQty
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
        /// <summary>
        /// ดึงข้อมูล LOT พร้อม Quantity จาก Output_Defect
        /// </summary>
        [HttpGet("get-lots-with-quantity")]
        public async Task<IActionResult> GetLotsWithQuantity([FromQuery] string? mcNo, [FromQuery] DateTime? date)
        {
            try
            {
                var targetDate = date?.Date ?? DateTime.UtcNow.Date;

                // ดึงข้อมูลจาก ThRecord
                var thRecordsQuery = _thicknessContext.ThRecords.AsQueryable();

                if (!string.IsNullOrEmpty(mcNo))
                {
                    thRecordsQuery = thRecordsQuery.Where(t => t.McPo == mcNo);
                }

                var thRecords = await thRecordsQuery.ToListAsync();

                var result = new List<object>();

                foreach (var thRecord in thRecords)
                {
                    // สร้าง PO LOT
                    string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";
                    string status = thRecord.Status ?? "";
                    bool checkSt = false;
                    int quantity = 0;

                    // ตรวจสอบ Status
                    if (status.ToLower() == "rescreen")
                    {
                        // เช็คใน TH100Record
                        var th100Record = await _thicknessContext.Th100Records
                            .FirstOrDefaultAsync(t =>
                                t.LotPo == thRecord.LotPo &&
                                t.McPo == thRecord.McPo &&
                                t.NoPo == thRecord.NoPo);

                        if (th100Record != null && th100Record.Status?.ToUpper() == "OK")
                        {
                            checkSt = true;
                            status = "OK (Rescreen)";
                        }
                    }
                    else if (!string.IsNullOrEmpty(status))
                    {
                        checkSt = status.ToUpper() == "OK";
                    }

                    // ดึงจำนวนจาก Output_Defect_A0400
                    var outputA0400 = await _outputContext.OutputDefectA0400s
                        .FirstOrDefaultAsync(o => o.Ltlotno == thRecord.ImobileLot);

                    if (outputA0400 != null)
                    {
                        quantity = Convert.ToInt32(outputA0400.Mfoutqn ?? 0);
                    }
                    else
                    {
                        // ถ้าไม่มีใน A0400 ให้เช็คใน A0600
                        var outputA0600 = await _outputContext.OutputDefectA0600s
                            .FirstOrDefaultAsync(o => o.Ltlotno == thRecord.ImobileLot);

                        if (outputA0600 != null)
                        {
                            quantity = Convert.ToInt32(outputA0400.Mfoutqn ?? 0);
                        }
                    }

                    result.Add(new
                    {
                        poLot = poLot,
                        imobileLot = thRecord.ImobileLot,
                        mcNo = thRecord.McPo,
                        status = status,
                        check = checkSt ? "OK" : "NG",
                        checkSt = checkSt,
                        quantity = quantity
                    });
                }

                // สรุปข้อมูล
                int totalLots = result.Count;
                int okCount = result.Count(r => ((dynamic)r).checkSt == true);
                int ngCount = result.Count(r => ((dynamic)r).checkSt == false);
                int totalQty = result.Sum(r => ((dynamic)r).quantity);

                return Ok(new
                {
                    success = true,
                    data = result,
                    summary = new
                    {
                        date = targetDate,
                        mcNo = mcNo,
                        totalLots = totalLots,
                        okCount = okCount,
                        ngCount = ngCount,
                        totalQuantity = totalQty
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูล",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// ดึงข้อมูล LOT เฉพาะ MC ของวันนี้ทั้งหมด (Real-time)
        /// </summary>
        [HttpGet("get-mc-lots-summary")]
        public async Task<IActionResult> GetMcLotsSummary([FromQuery] string mcNo)
        {
            try
            {
                if (string.IsNullOrEmpty(mcNo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "กรุณาระบุ MC Number"
                    });
                }

                var today = DateTime.UtcNow.Date;

                // ดึงข้อมูลจาก ThRecord สำหรับ MC นี้ของวันนี้เท่านั้น
                var thRecords = await _thicknessContext.ThRecords
                    .Where(t => t.McPo == mcNo && t.DateProcess.Date == today)
                    .OrderByDescending(t => t.DateProcess)
                    .ThenByDescending(t => t.NoPo)
                    .ToListAsync();

                var lotList = new List<object>();

                foreach (var thRecord in thRecords)
                {
                    string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";
                    string status = thRecord.Status ?? "";
                    bool checkSt = false;
                    int quantity = 0;

                    // ตรวจสอบ Status
                    if (status.ToLower() == "rescreen")
                    {
                        var th100Record = await _thicknessContext.Th100Records
                            .FirstOrDefaultAsync(t =>
                                t.LotPo == thRecord.LotPo &&
                                t.McPo == thRecord.McPo &&
                                t.NoPo == thRecord.NoPo);

                        if (th100Record != null && th100Record.Status?.ToUpper() == "OK")
                        {
                            checkSt = true;
                            status = "OK (Rescreen)";
                        }
                    }
                    else if (!string.IsNullOrEmpty(status))
                    {
                        checkSt = status.ToUpper() == "OK";
                    }

                    // ดึงจำนวน
                    var outputA0400 = await _outputContext.OutputDefectA0400s
                        .FirstOrDefaultAsync(o => o.Ltlotno == thRecord.ImobileLot);

                    if (outputA0400 != null)
                    {
                        quantity = Convert.ToInt32(outputA0400.Mfoutqn ?? 0);
                    }
                    else
                    {
                        var outputA0600 = await _outputContext.OutputDefectA0600s
                            .FirstOrDefaultAsync(o => o.Ltlotno == thRecord.ImobileLot);

                        if (outputA0600 != null)
                        {
                            quantity = Convert.ToInt32(outputA0400.Mfoutqn ?? 0);
                        }
                    }

                    lotList.Add(new
                    {
                        poLot = poLot,
                        imobileLot = thRecord.ImobileLot,
                        status = status,
                        check = checkSt ? "OK" : "NG",
                        checkSt = checkSt,
                        quantity = quantity
                    });
                }

                // เรียงลำดับ LOT ทั้งหมดของวันนี้
                var sortedLots = lotList
                    .Select(lot => new
                    {
                        Lot = lot,
                        LastPart = ((dynamic)lot).poLot.Split('-').LastOrDefault() ?? "",
                    })
                    .Select(x => new
                    {
                        x.Lot,
                        x.LastPart,
                        Number = int.TryParse(System.Text.RegularExpressions.Regex.Match(x.LastPart, @"^\d+").Value, out int num) ? num : 0,
                        Letter = System.Text.RegularExpressions.Regex.Match(x.LastPart, @"[A-Za-z]+$").Value
                    })
                    .OrderByDescending(x => x.Number)
                    .ThenByDescending(x => x.Letter)
                    .Select(x => x.Lot)
                    .Reverse()
                    .ToList();

                // สรุปข้อมูล
                int okCount = sortedLots.Count(r => ((dynamic)r).checkSt == true);
                int ngCount = sortedLots.Count(r => ((dynamic)r).checkSt == false);
                int totalQty = sortedLots.Sum(r => ((dynamic)r).quantity);

                return Ok(new
                {
                    success = true,
                    data = sortedLots,
                    summary = new
                    {
                        mcNo = mcNo,
                        date = today,
                        totalCount = sortedLots.Count,
                        okCount = okCount,
                        ngCount = ngCount,
                        totalQuantity = totalQty
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูล MC",
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
}