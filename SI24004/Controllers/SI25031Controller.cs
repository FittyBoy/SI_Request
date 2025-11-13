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
        private readonly sqlServerContext _context; // เปลี่ยนเป็นชื่อ DbContext ของคุณ
        private readonly PostgrestContext _postgrerestContext;

        public SI25031Controller(sqlServerContext context, PostgrestContext postgrestContext)
        {
            _context = context;
            _postgrerestContext = postgrestContext;
        }

        /// <summary>
        /// ตรวจสอบและบันทึกข้อมูล Flow-out Prevention
        /// </summary>
        [HttpPost("check-and-save")]
        public async Task<IActionResult> CheckAndSaveFlowOut()
        {
            try
            {
                var savedRecords = new List<PoCheckFlow>();

                // ดึงข้อมูลทั้งหมดจาก ThRecord
                var thRecords = await _context.ThRecords.ToListAsync();

                foreach (var thRecord in thRecords)
                {
                    bool shouldSave = false;
                    bool checkSt = false;

                    // ถ้า status เป็น "rescreen" ให้เช็คใน Th100Record
                    if (thRecord.Status?.ToLower() == "rescreen")
                    {
                        // เช็คว่ามีข้อมูลใน Th100Record หรือไม่
                        var th100Record = await _context.Th100Records
                            .FirstOrDefaultAsync(t =>
                                t.LotPo == thRecord.LotPo &&
                                t.McPo == thRecord.McPo &&
                                t.NoPo == thRecord.NoPo);

                        // ถ้ามีข้อมูลและ status เป็น "OK"
                        if (th100Record != null && th100Record.Status?.ToUpper() == "OK")
                        {
                            shouldSave = true;
                            checkSt = true;
                        }
                    }
                    else if (!string.IsNullOrEmpty(thRecord.Status))
                    {
                        // ถ้า status เป็นค่าอื่นๆ ให้บันทึก
                        shouldSave = true;
                        checkSt = thRecord.Status.ToUpper() == "OK";
                    }

                    // บันทึกข้อมูลลง po_check_flow
                    if (shouldSave)
                    {
                        var poCheckFlow = new PoCheckFlow
                        {
                            PoLot = $"{thRecord.LotPo}{thRecord.McPo}{thRecord.NoPo}",
                            StatusTn = thRecord.Status,
                            CheckSt = checkSt,
                            CheckDate = DateTime.Now,
                            McNo = thRecord.McPo
                        };

                        _postgrerestContext.PoCheckFlows.Add(poCheckFlow);
                        savedRecords.Add(poCheckFlow);
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
                return BadRequest(new
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
                var savedRecords = new List<PoCheckFlow>();

                // ดึงข้อมูลจาก ThRecord ตาม MC และวันที่
                var query = _context.ThRecords.AsQueryable();

                if (!string.IsNullOrEmpty(request.McPo))
                {
                    query = query.Where(t => t.McPo == request.McPo);
                }

                if (request.DateProcess.HasValue)
                {
                    query = query.Where(t => t.DateProcess.Date == request.DateProcess.Value.Date);
                }

                var thRecords = await query.ToListAsync();

                foreach (var thRecord in thRecords)
                {
                    bool shouldSave = false;
                    bool checkSt = false;

                    if (thRecord.Status?.ToLower() == "rescreen")
                    {
                        var th100Record = await _context.Th100Records
                            .FirstOrDefaultAsync(t =>
                                t.LotPo == thRecord.LotPo &&
                                t.McPo == thRecord.McPo &&
                                t.NoPo == thRecord.NoPo);

                        if (th100Record != null && th100Record.Status?.ToUpper() == "OK")
                        {
                            shouldSave = true;
                            checkSt = true;
                        }
                    }
                    else if (!string.IsNullOrEmpty(thRecord.Status))
                    {
                        shouldSave = true;
                        checkSt = thRecord.Status.ToUpper() == "OK";
                    }

                    if (shouldSave)
                    {
                        var poCheckFlow = new PoCheckFlow
                        {
                            PoLot = $"{thRecord.LotPo}{thRecord.McPo}{thRecord.NoPo}",
                            StatusTn = thRecord.Status,
                            CheckSt = checkSt,
                            CheckDate = DateTime.Now,
                            McNo = thRecord.McPo
                        };

                        _postgrerestContext.PoCheckFlows.Add(poCheckFlow);
                        savedRecords.Add(poCheckFlow);
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
                return BadRequest(new
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
                var records = await _postgrerestContext.PoCheckFlows
                    .OrderByDescending(p => p.CheckDate)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = records
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูล",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ดึงข้อมูลตาม MC และวันที่
        /// </summary>
        [HttpGet("get-by-mc")]
        public async Task<IActionResult> GetFlowOutChecksByMc([FromQuery] string mcNo, [FromQuery] DateTime? date)
        {
            try
            {
                var query = _postgrerestContext.PoCheckFlows.AsQueryable();

                if (!string.IsNullOrEmpty(mcNo))
                {
                    query = query.Where(p => p.McNo == mcNo);
                }

                if (date.HasValue)
                {
                    query = query.Where(p => p.CheckDate == date.Value);
                }

                var records = await query
                    .OrderByDescending(p => p.CheckDate)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = records
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูล",
                    error = ex.Message
                });
            }
        }
    }

    // Request Model
    public class CheckFlowRequest
    {
        public string? McPo { get; set; }
        public DateTime? DateProcess { get; set; }
    }
}