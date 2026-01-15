using iText.Commons.Actions.Contexts;
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
        /// ค้นหา LOT จาก ThRecord พร้อมตรวจสอบลำดับ (ไม่บันทึกทันที รอ confirm จาก Modal)
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

                // ===============================================
                // ⭐ ตรวจสอบสถานะ HOLD และ SCRAP ก่อนเช็คลำดับ LOT
                // ===============================================
                if (!string.IsNullOrEmpty(thRecord.Status))
                {
                    var statusLower = thRecord.Status.ToLower();

                    if (statusLower == "hold")
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "❌ LOT นี้อยู่ในสถานะ HOLD\n\n⚠️ ไม่สามารถบันทึกเข้าระบบได้\n\n📌 กรุณาส่งกลับไปตรวจสอบ",
                            status = "HOLD",
                            canSave = false,
                            reason = "LOT อยู่ในสถานะ HOLD"
                        });
                    }

                    if (statusLower == "scrap")
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "❌ LOT นี้อยู่ในสถานะ SCRAP\n\n⚠️ ไม่สามารถบันทึกเข้าระบบได้\n\n📌 กรุณาส่งกลับ",
                            status = "SCRAP",
                            canSave = false,
                            reason = "LOT อยู่ในสถานะ SCRAP"
                        });
                    }
                }

                // ===============================================
                // ตรวจสอบลำดับ LOT
                // ===============================================
                var targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

                // ดึง LOT ทั้งหมดที่มี lot_po และ mc_po เดียวกันในวันนี้
                var existingLots = await _context.PoCheckFlows
                    .Where(p =>
                        p.CheckDate.HasValue &&
                        p.CheckDate.Value.Date == targetDate.Date &&
                        p.McNo == thRecord.McPo &&
                        p.PoLot.StartsWith($"{thRecord.LotPo}-{thRecord.McPo}-"))
                    .ToListAsync();

                // แยกเลข no_po จาก PoLot ที่มีอยู่
                var existingNoPos = existingLots
                    .Select(p => {
                        var parts = p.PoLot?.Split('-');
                        if (parts != null && parts.Length >= 3)
                        {
                            var thirdPart = parts[2];
                            // รองรับทั้งรูปแบบ "003-T" และ "H01-E" 
                            var numberMatch = System.Text.RegularExpressions.Regex.Match(thirdPart, @"[A-Z]?(\d+)");
                            if (numberMatch.Success && int.TryParse(numberMatch.Groups[1].Value, out int num))
                                return num;
                        }
                        return -1;
                    })
                    .Where(n => n > 0)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                // แยกเลข no_po ของ LOT ที่จะเพิ่ม (รองรับทั้ง "003-T", "H01-E", "017-C")
                var currentNoPoPart = thRecord.NoPo ?? "";
                var currentNoPoMatch = System.Text.RegularExpressions.Regex.Match(currentNoPoPart, @"[A-Z]?(\d+)");

                if (!currentNoPoMatch.Success || !int.TryParse(currentNoPoMatch.Groups[1].Value, out int currentNoPo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"รูปแบบ NO PO ไม่ถูกต้อง (ได้รับ: {thRecord.NoPo})"
                    });
                }

                // สร้าง PO LOT สำหรับแสดงในข้อความ
                string currentPoLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";
                string statusInfo = !string.IsNullOrEmpty(thRecord.Status) ? $"\n📊 Status: {thRecord.Status.ToUpper()}" : "";

                // *** เช็คว่าไม่มี LOT เลย ***
                if (!existingNoPos.Any())
                {
                    if (currentNoPo != 1)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"❌ ไม่มี LOT ของ {thRecord.LotPo}-{thRecord.McPo} ในระบบ\n\n📋 กรุณาเริ่มต้นด้วย: {thRecord.LotPo}-{thRecord.McPo}-001\n\n🔍 LOT ที่พยายามเพิ่ม: {currentPoLot}{statusInfo}",
                            requiredLot = $"{thRecord.LotPo}-{thRecord.McPo}-001",
                            currentLot = currentPoLot,
                            status = thRecord.Status,
                            reason = "ไม่มี LOT ในระบบ ต้องเริ่มด้วย 001"
                        });
                    }
                }
                else
                {
                    // *** เช็คว่ามี LOT ที่ข้ามหรือไม่ ***
                    var missingNoPos = new List<int>();
                    for (int i = 1; i < currentNoPo; i++)
                    {
                        if (!existingNoPos.Contains(i))
                        {
                            missingNoPos.Add(i);
                        }
                    }

                    if (missingNoPos.Any())
                    {
                        var missingLotsList = string.Join("\n• ", missingNoPos.Select(n => $"{thRecord.LotPo}-{thRecord.McPo}-{n:D3}"));

                        return BadRequest(new
                        {
                            success = false,
                            message = $"❌ ไม่สามารถเพิ่ม LOT นี้ได้\n\n⚠️ ยังมี LOT ก่อนหน้าที่ยังไม่ได้เพิ่ม:\n• {missingLotsList}\n\n🔍 LOT ที่พยายามเพิ่ม: {currentPoLot}{statusInfo}\n\n📌 กรุณาเพิ่ม LOT ตามลำดับก่อน",
                            missingLots = missingNoPos.Select(n => $"{thRecord.LotPo}-{thRecord.McPo}-{n:D3}").ToList(),
                            currentLot = currentPoLot,
                            status = thRecord.Status,
                            reason = "ข้าม LOT ที่ยังไม่ได้เพิ่ม"
                        });
                    }
                }

                // ===============================================
                // ถ้าผ่านการตรวจสอบลำดับแล้ว ตรวจสอบสถานะสำหรับเปิด Modal
                // ===============================================
                bool checkSt = false;
                bool hasTH100 = false;
                string finalStatus = thRecord.Status;
                string th100Status = null;

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
                        p.CheckDate == targetDate);

                return Ok(new
                {
                    success = true,
                    message = "✅ พบข้อมูล LOT และผ่านการตรวจสอบลำดับ",
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
                        existingQty = existingRecord?.LotQty,
                        sequenceInfo = new
                        {
                            currentNo = currentNoPo,
                            existingNos = existingNoPos,
                            isFirstLot = !existingNoPos.Any()
                        }
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
        /// บันทึก LOT พร้อม Qty หลังจาก confirm จาก Modal (ตรวจสอบอีกครั้งเพื่อความปลอดภัย)
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

                // ===============================================
                // ตรวจสอบลำดับ LOT อีกครั้ง (Double Check)
                // ===============================================
                var targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

                var existingLots = await _context.PoCheckFlows
                    .Where(p =>
                        p.CheckDate.HasValue &&
                        p.CheckDate.Value.Date == targetDate.Date &&
                        p.McNo == thRecord.McPo &&
                        p.PoLot.StartsWith($"{thRecord.LotPo}-{thRecord.McPo}-"))
                    .ToListAsync();

                // แยกเลขจาก no_po (รองรับทั้ง "003-T", "H01-E", "017-C")
                var existingNoPos = existingLots
                    .Select(p => {
                        var parts = p.PoLot?.Split('-');
                        if (parts != null && parts.Length >= 3)
                        {
                            var thirdPart = parts[2];
                            // รองรับทั้งรูปแบบ "003-T" และ "H01-E"
                            var numberMatch = System.Text.RegularExpressions.Regex.Match(thirdPart, @"[A-Z]?(\d+)");
                            if (numberMatch.Success && int.TryParse(numberMatch.Groups[1].Value, out int num))
                                return num;
                        }
                        return -1;
                    })
                    .Where(n => n > 0)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                // แยกเลข no_po ของ LOT ที่จะเพิ่ม (รองรับทั้ง "003-T", "H01-E", "017-C")
                var currentNoPoPart = thRecord.NoPo ?? "";
                var currentNoPoMatch = System.Text.RegularExpressions.Regex.Match(currentNoPoPart, @"[A-Z]?(\d+)");

                if (!currentNoPoMatch.Success || !int.TryParse(currentNoPoMatch.Groups[1].Value, out int currentNoPo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"รูปแบบ NO PO ไม่ถูกต้อง (ได้รับ: {thRecord.NoPo})"
                    });
                }

                // Double check ลำดับ
                if (!existingNoPos.Any())
                {
                    if (currentNoPo != 1)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"ไม่มี LOT ของ {thRecord.LotPo}-{thRecord.McPo} ในระบบ กรุณาเริ่มต้นด้วย {thRecord.LotPo}-{thRecord.McPo}-001"
                        });
                    }
                }
                else
                {
                    var missingNoPos = new List<int>();
                    for (int i = 1; i < currentNoPo; i++)
                    {
                        if (!existingNoPos.Contains(i))
                        {
                            missingNoPos.Add(i);
                        }
                    }

                    if (missingNoPos.Any())
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"ไม่สามารถเพิ่ม LOT นี้ได้ เนื่องจากยังมี LOT ก่อนหน้าที่ยังไม่ได้เพิ่ม: {string.Join(", ", missingNoPos.Select(n => $"{thRecord.LotPo}-{thRecord.McPo}-{n:D3}"))}"
                        });
                    }
                }

                // ===============================================
                // ตรวจสอบสถานะ LOT
                // ===============================================
                bool checkSt = false;
                string finalStatus = thRecord.Status;

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

                // ===============================================
                // บันทึกข้อมูล
                // ===============================================
                var existingRecord = await _context.PoCheckFlows
                    .FirstOrDefaultAsync(p =>
                        p.Imobilelot == thRecord.ImobileLot &&
                        p.CheckDate == targetDate);

                PoCheckFlow poCheckFlow;

                if (existingRecord != null)
                {
                    existingRecord.StatusTn = finalStatus;
                    existingRecord.CheckSt = checkSt;
                    existingRecord.CheckDate = targetDate;
                    existingRecord.McNo = thRecord.McPo;
                    existingRecord.PoLot = poLot;
                    existingRecord.LotQty = totalQty;
                    poCheckFlow = existingRecord;
                }
                else
                {
                    poCheckFlow = new PoCheckFlow
                    {
                        PoLot = poLot,
                        Imobilelot = thRecord.ImobileLot,
                        StatusTn = finalStatus,
                        CheckSt = checkSt,
                        CheckDate = targetDate,
                        McNo = thRecord.McPo,
                        LotQty = totalQty
                    };
                    _context.PoCheckFlows.Add(poCheckFlow);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"✅ บันทึก LOT {poLot} สำเร็จ",
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
        /// ดึงข้อมูล LOT ตาม MC (แสดงเฉพาะ 8 LOT ล่าสุดต่อ MC) เรียงตามเลข 3 ตัวท้าย (รองรับตัวอักษร)
        /// </summary>
        [HttpGet("get-lots-by-mc")]
        public async Task<IActionResult> GetLotsByMc([FromQuery] string mcNo, [FromQuery] string? date)
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

                DateTime targetDate;

                // แปลง string date เป็น DateTime
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                }
                else
                {
                    targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                }

                var records = await _context.PoCheckFlows
                                            .Where(p => p.McNo == mcNo &&
                                                   p.CheckDate.HasValue &&
                                                   p.CheckDate.Value.Year == targetDate.Year &&
                                                   p.CheckDate.Value.Month == targetDate.Month &&
                                                   p.CheckDate.Value.Day == targetDate.Day)
                                            .ToListAsync();

                // ถ้าไม่มีข้อมูล
                if (records.Count == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        data = new List<object>(),
                        totalCount = 0,
                        displayCount = 0,
                        okCount = 0,
                        ngCount = 0,
                        message = $"ไม่พบข้อมูล LOT ของ MC {mcNo} ในวันที่ {targetDate:yyyy-MM-dd}"
                    });
                }

                // นับจำนวน LOT ทั้งหมดของ MC นี้ในวันที่เลือก
                int totalCount = records.Count;
                int okCount = records.Count(r => r.CheckSt);
                int ngCount = records.Count(r => !r.CheckSt);

                // เรียงลำดับตาม LOT โดยแยกเลขและตัวอักษร (รองรับทั้ง "003-T", "H01-E", "017-C")
                // เรียงจาก LOT น้อย → มาก (เลขน้อยอยู่บน)
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
                        // ดึงเลขจากรูปแบบ "H01-E" หรือ "003-T"
                        NumberMatch = System.Text.RegularExpressions.Regex.Match(x.LastPart, @"[A-Z]?(\d+)"),
                        // ดึงตัวอักษรต่อท้าย เช่น "-T", "-E"
                        Letter = System.Text.RegularExpressions.Regex.Match(x.LastPart, @"-([A-Z]+)$").Groups[1].Value
                    })
                    .Select(x => new
                    {
                        x.Record,
                        Number = x.NumberMatch.Success && int.TryParse(x.NumberMatch.Groups[1].Value, out int num) ? num : 0,
                        x.Letter
                    })
                    .OrderBy(x => x.Number)        // เรียงจากน้อยไปมาก (LOT เก่าก่อน)
                    .ThenBy(x => x.Letter)         // ถ้าเลขเท่ากัน เรียงตามตัวอักษร A, B, C...
                    .Take(8)                       // เอาเฉพาะ 8 LOT แรก
                    .Select(x => x.Record)
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
                    totalCount = totalCount,           // จำนวน LOT ทั้งหมดของ MC นี้ในวันที่เลือก
                    displayCount = sortedRecords.Count, // จำนวนที่แสดง (สูงสุด 8)
                    okCount = okCount,                 // OK ทั้งหมดของ MC นี้
                    ngCount = ngCount,                 // NG ทั้งหมดของ MC นี้
                    date = targetDate.ToString("yyyy-MM-dd")
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
        public async Task<IActionResult> GetMcList([FromQuery] string? date)
        {
            try
            {
                DateTime targetDate;

                // แปลง string date เป็น DateTime
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                }
                else
                {
                    targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                }

                // ดึง MC จาก ThRecords ตามวันที่
                var mcList = await _thicknessContext.ThRecords
                    .Where(t => !string.IsNullOrEmpty(t.McPo) && t.DateProcess.Date == targetDate.Date)
                    .Select(t => t.McPo)
                    .Distinct()
                    .OrderBy(mc => mc)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = mcList,
                    date = targetDate.ToString("yyyy-MM-dd"),
                    count = mcList.Count
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

                var thRecords = await thRecordsQuery.Where(x => x.DateProcess.Date == date.Value.Date).ToListAsync();

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
                            quantity = Convert.ToInt32(outputA0600.Mfoutqn ?? 0);
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
                            quantity = Convert.ToInt32(outputA0600.Mfoutqn ?? 0);
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
        /// <summary>
        /// ดึงรายการ MC ทั้งหมดจาก ThRecord (ตามวันที่)
        /// </summary>
        [HttpGet("get-mc-list-from-threcord")]
        public async Task<IActionResult> GetMcListFromThRecord([FromQuery] string? date)
        {
            try
            {
                DateTime targetDate;

                // แปลง string เป็น DateTime แบบ UTC
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                }
                else
                {
                    targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                }

                // ดึง MC จาก PoCheckFlows ตามวันที่
                var mcList = await _context.PoCheckFlows
                    .Where(p => p.CheckDate.HasValue && p.CheckDate.Value.Date == targetDate.Date)
                    .Select(p => p.McNo)
                    .Distinct()
                    .OrderBy(mc => mc)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = mcList,
                    date = targetDate.ToString("yyyy-MM-dd"),
                    count = mcList.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงรายการ MC จาก PoCheckFlow",
                    error = ex.Message
                });
            }
        }


        /// <summary>
        /// ดึงรายการ MC ทั้งหมดจาก ThRecord (ทุกวัน - ไม่กรองวันที่)
        /// </summary>
        [HttpGet("get-all-mc-from-threcord")]
        public async Task<IActionResult> GetAllMcFromThRecord()
        {
            try
            {
                // ดึง MC จาก ThRecords ทั้งหมด
                var mcList = await _thicknessContext.ThRecords
                    .Where(t => !string.IsNullOrEmpty(t.McPo))
                    .Select(t => t.McPo)
                    .Distinct()
                    .OrderBy(mc => mc)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = mcList,
                    count = mcList.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงรายการ MC จาก ThRecord",
                    error = ex.Message
                });
            }
        }
        /// <summary>
        /// ดึงข้อมูล LOT ทั้งหมดจาก ThRecord พร้อม Quantity แยกตาม MC (สำหรับ Summary Dashboard)
        /// </summary>
        [HttpGet("get-lots-summary-by-mc")]
        public async Task<IActionResult> GetLotsSummaryByMc([FromQuery] DateTime? date)
        {
            try
            {
                var targetDate = date?.Date ?? DateTime.UtcNow.Date;

                // ดึงข้อมูลจาก ThRecord ตามวันที่
                var thRecords = await _thicknessContext.ThRecords
                    .Where(t => t.DateProcess.Date == targetDate)
                    .OrderBy(t => t.McPo)
                    .ThenBy(t => t.NoPo)
                    .ToListAsync();

                var result = new List<object>();
                var mcSummary = new Dictionary<string, dynamic>();

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

                    // ดึงจำนวนจาก Output_Defect
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
                            quantity = Convert.ToInt32(outputA0600.Mfoutqn ?? 0);
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

                    // สรุปตาม MC
                    if (!string.IsNullOrEmpty(thRecord.McPo))
                    {
                        if (!mcSummary.ContainsKey(thRecord.McPo))
                        {
                            mcSummary[thRecord.McPo] = new
                            {
                                mcNo = thRecord.McPo,
                                totalLots = 0,
                                okCount = 0,
                                ngCount = 0,
                                totalQty = 0
                            };
                        }

                        var current = mcSummary[thRecord.McPo];
                        mcSummary[thRecord.McPo] = new
                        {
                            mcNo = thRecord.McPo,
                            totalLots = current.totalLots + 1,
                            okCount = current.okCount + (checkSt ? 1 : 0),
                            ngCount = current.ngCount + (checkSt ? 0 : 1),
                            totalQty = current.totalQty + quantity
                        };
                    }
                }

                // สรุปรวม
                int totalLots = result.Count;
                int okCount = result.Count(r => ((dynamic)r).checkSt == true);
                int ngCount = result.Count(r => ((dynamic)r).checkSt == false);
                int totalQty = result.Sum(r => (int)((dynamic)r).quantity);

                return Ok(new
                {
                    success = true,
                    data = result,
                    mcList = mcSummary.Keys.OrderBy(k => k).ToList(),
                    mcSummary = mcSummary.Values.ToList(),
                    summary = new
                    {
                        date = targetDate,
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
        /// ดึงข้อมูล LOT ตาม MC จาก ThRecord (Real-time)
        /// </summary>
        [HttpGet("get-lots-by-mc-from-threcord")]
        public async Task<IActionResult> GetLotsByMcFromThRecord([FromQuery] string mcNo, [FromQuery] string? date)
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

                DateTime targetDate;

                // แปลง string เป็น DateTime แบบ UTC
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                }
                else
                {
                    targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                }

                // ดึงข้อมูลจาก PoCheckFlow
                var records = await _context.PoCheckFlows
                    .Where(p => p.McNo == mcNo && p.CheckDate.HasValue && p.CheckDate.Value.Date == targetDate.Date)
                    .OrderBy(p => p.PoLot)
                    .ToListAsync();

                var lotList = records.Select(r => new
                {
                    id = r.Id,
                    poLot = r.PoLot,
                    imobileLot = r.Imobilelot,
                    mcNo = r.McNo,
                    status = r.StatusTn,
                    statusTn = r.StatusTn,
                    check = r.CheckSt ? "OK" : "NG",
                    checkSt = r.CheckSt,
                    quantity = r.LotQty,
                    lotQty = r.LotQty,
                    checkDate = r.CheckDate
                }).ToList();

                // สรุป
                int okCount = lotList.Count(r => r.checkSt);
                int ngCount = lotList.Count(r => !r.checkSt);
                int totalQty = lotList.Sum(r => r.quantity ?? 0);

                return Ok(new
                {
                    success = true,
                    data = lotList,
                    summary = new
                    {
                        mcNo = mcNo,
                        date = targetDate.ToString("yyyy-MM-dd"),
                        totalLots = lotList.Count,
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
                    error = ex.Message
                });
            }
        }
        [HttpGet("get-all-lots-by-date")]
        public async Task<IActionResult> GetAllLotsByDate([FromQuery] string? date)
        {
            try
            {
                DateTime targetDate;

                // แปลง string เป็น DateTime แบบ UTC
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                }
                else
                {
                    targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                }

                var records = await _context.PoCheckFlows
                    .Where(p => p.CheckDate.HasValue && p.CheckDate.Value.Date == targetDate.Date)
                    .OrderBy(p => p.McNo)
                    .ThenBy(p => p.PoLot)
                    .ToListAsync();

                var lotList = records.Select(r => new
                {
                    id = r.Id,
                    poLot = r.PoLot,
                    imobileLot = r.Imobilelot,
                    mcNo = r.McNo,
                    status = r.StatusTn,
                    statusTn = r.StatusTn,
                    check = r.CheckSt ? "OK" : "NG",
                    checkSt = r.CheckSt,
                    quantity = r.LotQty,
                    lotQty = r.LotQty,
                    checkDate = r.CheckDate
                }).ToList();

                int okCount = lotList.Count(r => r.checkSt);
                int ngCount = lotList.Count(r => !r.checkSt);
                int totalQty = lotList.Sum(r => r.quantity ?? 0);

                return Ok(new
                {
                    success = true,
                    data = lotList,
                    summary = new
                    {
                        date = targetDate.ToString("yyyy-MM-dd"),
                        totalLots = lotList.Count,
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