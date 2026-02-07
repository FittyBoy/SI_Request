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
        /// ✅ FIX: ค้นหา LOT โดยเช็ค LotPo (วันจริง) และรองรับ NoPo ที่ไม่ตรงกัน
        /// </summary>
        [HttpPost("search-lot")]
        public async Task<IActionResult> SearchLot([FromBody] SearchLotRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.LotNumber))
                {
                    return BadRequest(new { success = false, message = "กรุณาระบุ LOT Number" });
                }

                var thRecord = await _thicknessContext.ThRecords
                    .FirstOrDefaultAsync(t => t.ImobileLot == request.LotNumber);

                if (thRecord == null)
                {
                    return NotFound(new { success = false, message = $"ไม่พบข้อมูล ImobileLot: {request.LotNumber}" });
                }

                // ตรวจสอบสถานะ HOLD และ SCRAP
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

                // ✅ FIX: กรอง LOT เฉพาะของวันเดียวกัน (ตาม LotPo) ไม่ใช่ CheckDate
                var targetLotPoPrefix = thRecord.LotPo; // เช่น "0502", "0602"

                var existingLots = await _context.PoCheckFlows
                    .Where(p =>
                        p.McNo == thRecord.McPo &&
                        p.PoLot.StartsWith($"{targetLotPoPrefix}-{thRecord.McPo}-"))
                    .ToListAsync();

                var existingNoPos = existingLots
                    .Select(p =>
                    {
                        var parts = p.PoLot?.Split('-');
                        if (parts != null && parts.Length >= 3)
                        {
                            var thirdPart = parts[2];
                            // ✅ ตัดเฉพาะตัวเลข ไม่สน suffix (-E, -U)
                            var numberMatch = System.Text.RegularExpressions.Regex.Match(thirdPart, @"^(\d+)");
                            if (numberMatch.Success && int.TryParse(numberMatch.Groups[1].Value, out int num))
                                return num;
                        }
                        return -1;
                    })
                    .Where(n => n > 0)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                var currentNoPoPart = thRecord.NoPo ?? "";
                var currentNoPoMatch = System.Text.RegularExpressions.Regex.Match(currentNoPoPart, @"^(\d+)");

                if (!currentNoPoMatch.Success || !int.TryParse(currentNoPoMatch.Groups[1].Value, out int currentNoPo))
                {
                    return BadRequest(new { success = false, message = $"รูปแบบ NO PO ไม่ถูกต้อง (ได้รับ: {thRecord.NoPo})" });
                }

                string currentPoLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";
                string statusInfo = !string.IsNullOrEmpty(thRecord.Status) ? $"\n📊 Status: {thRecord.Status.ToUpper()}" : "";

                var autoAddedScrapLots = new List<string>();

                // ✅ กรณีไม่มี LOT เลยในระบบ
                if (!existingNoPos.Any())
                {
                    if (currentNoPo > 1)
                    {
                        var missingNormalLots = new List<int>();

                        for (int i = 1; i < currentNoPo; i++)
                        {
                            var lotStatus = await CheckLotStatus(thRecord.LotPo, thRecord.McPo, i);

                            if (lotStatus.isScrap && lotStatus.thRecord != null)
                            {
                                // 🔥 Auto-add SCRAP LOT
                                await AutoAddScrapLot(lotStatus.thRecord);
                                autoAddedScrapLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{lotStatus.thRecord.NoPo}");
                            }
                            else if (!lotStatus.isHold)
                            {
                                missingNormalLots.Add(i);
                            }
                        }

                        if (missingNormalLots.Any())
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

                        if (autoAddedScrapLots.Any())
                        {
                            statusInfo += $"\n\n✅ Auto-add SCRAP LOT:\n• {string.Join("\n• ", autoAddedScrapLots)}";
                        }
                    }
                }
                else
                {
                    // ✅ กรณีมี LOT บางตัวในระบบแล้ว
                    var missingNormalLots = new List<int>();

                    for (int i = 1; i < currentNoPo; i++)
                    {
                        if (!existingNoPos.Contains(i))
                        {
                            var lotStatus = await CheckLotStatus(thRecord.LotPo, thRecord.McPo, i);

                            if (lotStatus.isScrap && lotStatus.thRecord != null)
                            {
                                // 🔥 Auto-add SCRAP LOT
                                await AutoAddScrapLot(lotStatus.thRecord);
                                autoAddedScrapLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{lotStatus.thRecord.NoPo}");
                            }
                            else if (!lotStatus.isHold)
                            {
                                missingNormalLots.Add(i);
                            }
                        }
                    }

                    // ❌ ถ้ายังมี LOT ปกติที่ยังไม่ได้เพิ่ม → ไม่อนุญาต
                    if (missingNormalLots.Any())
                    {
                        var missingLotsList = string.Join("\n• ", missingNormalLots.Select(n => $"{thRecord.LotPo}-{thRecord.McPo}-{n:D3}"));

                        return BadRequest(new
                        {
                            success = false,
                            message = $"❌ ไม่สามารถเพิ่ม LOT นี้ได้\n\n⚠️ ยังมี LOT ก่อนหน้าที่ยังไม่ได้เพิ่ม:\n• {missingLotsList}\n\n🔍 LOT ที่พยายามเพิ่ม: {currentPoLot}{statusInfo}\n\n📌 กรุณาเพิ่ม LOT ตามลำดับก่อน",
                            missingLots = missingNormalLots.Select(n => $"{thRecord.LotPo}-{thRecord.McPo}-{n:D3}").ToList(),
                            currentLot = currentPoLot,
                            status = thRecord.Status,
                            reason = "ข้าม LOT ที่ยังไม่ได้เพิ่ม"
                        });
                    }

                    // ✅ แสดงข้อมูล SCRAP ที่ Auto-add
                    if (autoAddedScrapLots.Any())
                    {
                        statusInfo += $"\n\n✅ Auto-add SCRAP LOT:\n• {string.Join("\n• ", autoAddedScrapLots)}";
                    }
                }

                // ✅ FIX: เช็ค TH100 โดยใช้เฉพาะตัวเลข NoPo (ไม่สน suffix)
                bool checkSt = false;
                bool hasTH100 = false;
                string finalStatus = thRecord.Status;
                string th100Status = null;

                if (thRecord.Status?.ToLower() == "rescreen")
                {
                    var th100Record = await FindTH100Record(thRecord);

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

                string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";

                var existingRecord = await _context.PoCheckFlows
                    .FirstOrDefaultAsync(p =>
                        p.Imobilelot == thRecord.ImobileLot);

                return Ok(new
                {
                    success = true,
                    message = "✅ พบข้อมูล LOT และผ่านการตรวจสอบลำดับ" + statusInfo,
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
                        autoAddedScrapLots = autoAddedScrapLots,
                        sequenceInfo = new
                        {
                            currentNo = currentNoPo,
                            existingNos = existingNoPos,
                            isFirstLot = !existingNoPos.Any(),
                            lotPoPrefix = targetLotPoPrefix
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการค้นหา", error = ex.Message });
            }
        }

        [HttpPost("save-lot")]
        public async Task<IActionResult> SaveLot([FromBody] SaveLotRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ImobileLot))
                {
                    return BadRequest(new { success = false, message = "กรุณาระบุ ImobileLot" });
                }

                var thRecord = await _thicknessContext.ThRecords
                    .FirstOrDefaultAsync(t => t.ImobileLot == request.ImobileLot);

                if (thRecord == null)
                {
                    return NotFound(new { success = false, message = "ไม่พบข้อมูล LOT" });
                }

                // ✅ FIX: กรอง LOT เฉพาะของวันเดียวกัน (ตาม LotPo)
                var targetLotPoPrefix = thRecord.LotPo;

                var existingLots = await _context.PoCheckFlows
                    .Where(p =>
                        p.McNo == thRecord.McPo &&
                        p.PoLot.StartsWith($"{targetLotPoPrefix}-{thRecord.McPo}-"))
                    .ToListAsync();

                var existingNoPos = existingLots
                    .Select(p =>
                    {
                        var parts = p.PoLot?.Split('-');
                        if (parts != null && parts.Length >= 3)
                        {
                            var thirdPart = parts[2];
                            var numberMatch = System.Text.RegularExpressions.Regex.Match(thirdPart, @"^(\d+)");
                            if (numberMatch.Success && int.TryParse(numberMatch.Groups[1].Value, out int num))
                                return num;
                        }
                        return -1;
                    })
                    .Where(n => n > 0)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                var currentNoPoPart = thRecord.NoPo ?? "";
                var currentNoPoMatch = System.Text.RegularExpressions.Regex.Match(currentNoPoPart, @"^(\d+)");

                if (!currentNoPoMatch.Success || !int.TryParse(currentNoPoMatch.Groups[1].Value, out int currentNoPo))
                {
                    return BadRequest(new { success = false, message = $"รูปแบบ NO PO ไม่ถูกต้อง (ได้รับ: {thRecord.NoPo})" });
                }

                var autoAddedScrapLots = new List<string>();

                // ✅ ตรวจสอบลำดับ LOT และ Auto-add SCRAP
                if (!existingNoPos.Any())
                {
                    if (currentNoPo > 1)
                    {
                        var missingNormalLots = new List<int>();

                        for (int i = 1; i < currentNoPo; i++)
                        {
                            var lotStatus = await CheckLotStatus(thRecord.LotPo, thRecord.McPo, i);

                            if (lotStatus.isScrap && lotStatus.thRecord != null)
                            {
                                await AutoAddScrapLot(lotStatus.thRecord);
                                autoAddedScrapLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{lotStatus.thRecord.NoPo}");
                            }
                            else if (!lotStatus.isHold)
                            {
                                missingNormalLots.Add(i);
                            }
                        }

                        if (missingNormalLots.Any())
                        {
                            return BadRequest(new
                            {
                                success = false,
                                message = $"ไม่มี LOT ของ {thRecord.LotPo}-{thRecord.McPo} ในระบบ กรุณาเริ่มต้นด้วย {thRecord.LotPo}-{thRecord.McPo}-001"
                            });
                        }
                    }
                }
                else
                {
                    var missingNormalLots = new List<int>();

                    for (int i = 1; i < currentNoPo; i++)
                    {
                        if (!existingNoPos.Contains(i))
                        {
                            var lotStatus = await CheckLotStatus(thRecord.LotPo, thRecord.McPo, i);

                            if (lotStatus.isScrap && lotStatus.thRecord != null)
                            {
                                await AutoAddScrapLot(lotStatus.thRecord);
                                autoAddedScrapLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{lotStatus.thRecord.NoPo}");
                            }
                            else if (!lotStatus.isHold)
                            {
                                missingNormalLots.Add(i);
                            }
                        }
                    }

                    if (missingNormalLots.Any())
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"ไม่สามารถเพิ่ม LOT นี้ได้ เนื่องจากยังมี LOT ก่อนหน้าที่ยังไม่ได้เพิ่ม: {string.Join(", ", missingNormalLots.Select(n => $"{thRecord.LotPo}-{thRecord.McPo}-{n:D3}"))}"
                        });
                    }
                }

                // ✅ FIX: เช็ค TH100 โดยไม่สน suffix
                bool checkSt = false;
                string finalStatus = thRecord.Status;

                if (thRecord.Status?.ToLower() == "rescreen")
                {
                    var th100Record = await FindTH100Record(thRecord);

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
                        return BadRequest(new { success = false, message = "LOT นี้ Rescreen Pending ไม่สามารถบันทึกได้" });
                    }
                }
                else if (!string.IsNullOrEmpty(thRecord.Status))
                {
                    checkSt = thRecord.Status.ToUpper() == "OK";

                    if (thRecord.Status.ToLower() == "hold" || thRecord.Status.ToLower() == "scrap")
                    {
                        return BadRequest(new { success = false, message = $"LOT นี้อยู่ในสถานะ {thRecord.Status} ไม่สามารถบันทึกได้" });
                    }
                }

                string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";
                int totalQty = request.LotQty ?? 0;

                if (totalQty <= 0)
                {
                    return BadRequest(new { success = false, message = "กรุณาระบุจำนวน Lot Qty" });
                }

                var existingRecord = await _context.PoCheckFlows
                    .FirstOrDefaultAsync(p => p.Imobilelot == thRecord.ImobileLot);

                PoCheckFlow poCheckFlow;
                var checkDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

                if (existingRecord != null)
                {
                    existingRecord.StatusTn = finalStatus;
                    existingRecord.CheckSt = checkSt;
                    existingRecord.CheckDate = checkDate;
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
                        CheckDate = checkDate,
                        McNo = thRecord.McPo,
                        LotQty = totalQty
                    };
                    _context.PoCheckFlows.Add(poCheckFlow);
                }

                await _context.SaveChangesAsync();

                var message = $"✅ บันทึก LOT {poLot} สำเร็จ";
                if (autoAddedScrapLots.Any())
                {
                    message += $"\n\n🔄 Auto-add SCRAP LOT:\n• {string.Join("\n• ", autoAddedScrapLots)}";
                }

                return Ok(new
                {
                    success = true,
                    message = message,
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
                        lotQty = poCheckFlow.LotQty,
                        autoAddedScrapLots = autoAddedScrapLots
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการบันทึก", error = ex.Message });
            }
        }

        /// <summary>
        /// ✅ FIX: ค้นหา TH100 โดยไม่สน suffix (-E, -U)
        /// </summary>
        private async Task<Th100Record?> FindTH100Record(ThRecord thRecord)
        {
            // ดึงเฉพาะตัวเลขจาก NoPo (ไม่สน -E, -U)
            var noPoNumber = System.Text.RegularExpressions.Regex.Match(thRecord.NoPo ?? "", @"^(\d+)").Groups[1].Value;

            if (string.IsNullOrEmpty(noPoNumber))
                return null;

            // ค้นหาโดยเทียบเฉพาะ LotPo, McPo และตัวเลขของ NoPo
            var th100List = await _thicknessContext.Th100Records
                .Where(t => t.LotPo == thRecord.LotPo && t.McPo == thRecord.McPo)
                .ToListAsync();

            foreach (var th100 in th100List)
            {
                var th100NoPoNumber = System.Text.RegularExpressions.Regex.Match(th100.NoPo ?? "", @"^(\d+)").Groups[1].Value;
                if (th100NoPoNumber == noPoNumber)
                {
                    return th100;
                }
            }

            // ถ้ายังไม่เจอ ลอง match ImobileLot
            return await _thicknessContext.Th100Records
                .FirstOrDefaultAsync(t => t.ImobileLot == thRecord.ImobileLot);
        }

        /// <summary>
        /// Auto-add SCRAP LOT with Qty = 0
        /// </summary>
        private async Task AutoAddScrapLot(ThRecord thRecord)
        {
            if (thRecord == null) return;

            string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";

            // ตรวจสอบว่ามีอยู่แล้วหรือไม่
            var existingRecord = await _context.PoCheckFlows
                .FirstOrDefaultAsync(p => p.Imobilelot == thRecord.ImobileLot);

            if (existingRecord == null)
            {
                var checkDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

                var scrapLot = new PoCheckFlow
                {
                    PoLot = poLot,
                    Imobilelot = thRecord.ImobileLot,
                    StatusTn = "SCRAP",
                    CheckSt = false,
                    CheckDate = checkDate,
                    McNo = thRecord.McPo,
                    LotQty = 0  // 🔥 SCRAP = จำนวน 0
                };

                _context.PoCheckFlows.Add(scrapLot);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// ✅ FIX: ตรวจสอบ LOT ว่าเป็น SCRAP, HOLD หรือไม่ (ใช้เฉพาะตัวเลข)
        /// </summary>
        private async Task<(bool isScrap, bool isHold, string status, ThRecord thRecord)> CheckLotStatus(string lotPo, string mcPo, int noPoNumber)
        {
            // ค้นหาโดยเทียบเฉพาะ LotPo, McPo และตัวเลขของ NoPo
            var thRecords = await _thicknessContext.ThRecords
                .Where(t => t.LotPo == lotPo && t.McPo == mcPo)
                .ToListAsync();

            foreach (var thRecord in thRecords)
            {
                var thNoPoNumber = System.Text.RegularExpressions.Regex.Match(thRecord.NoPo ?? "", @"^(\d+)").Groups[1].Value;

                if (int.TryParse(thNoPoNumber, out int num) && num == noPoNumber)
                {
                    if (!string.IsNullOrEmpty(thRecord.Status))
                    {
                        var statusLower = thRecord.Status.ToLower().Trim();

                        if (statusLower == "scrap")
                        {
                            return (true, false, thRecord.Status, thRecord);
                        }

                        if (statusLower == "hold")
                        {
                            return (false, true, thRecord.Status, thRecord);
                        }
                    }

                    return (false, false, "", thRecord);
                }
            }

            return (false, false, "", null);
        }

        /// <summary>
        /// ✅ FIX: ดึงข้อมูล LOT ตาม MC - แสดงเฉพาะวันที่ตรงกัน (DDMM) หรือวันที่บันทึก (CheckDate)
        /// รูปแบบ PoLot: DDMMO-MC-XXX (DD=วัน, MM=เดือน, O=ปี)
        /// </summary>
        [HttpGet("get-lots-by-mc")]
        public async Task<IActionResult> GetLotsByMc([FromQuery] string mcNo, [FromQuery] string? date)
        {
            try
            {
                if (string.IsNullOrEmpty(mcNo))
                {
                    return BadRequest(new { success = false, message = "กรุณาระบุ MC Number" });
                }

                DateTime targetDate;

                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                }
                else
                {
                    targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                }

                // ✅ PoLot format: DDMMO (เช่น 0602O = วันที่ 6 เดือน 02)
                // สร้าง DDMM pattern
                string targetDDMM = $"{targetDate.Day:D2}{targetDate.Month:D2}"; // เช่น "0602" = 6 กุมภาพันธ์

                // ✅ กรองด้วย OR condition:
                var records = await _context.PoCheckFlows
                    .Where(p => p.McNo == mcNo &&
                           (
                               // เงื่อนไขที่ 1: วันที่บันทึก (CheckDate)
                               (p.CheckDate.HasValue &&
                                p.CheckDate.Value.Year == targetDate.Year &&
                                p.CheckDate.Value.Month == targetDate.Month &&
                                p.CheckDate.Value.Day == targetDate.Day)
                               ||
                               // เงื่อนไขที่ 2: วันและเดือนตรงกัน (PoLot ตำแหน่งที่ 1-4 ต้องเป็น DDMM)
                               // Format: DDMMO-MC-XXX เช่น 0602O-638-001
                               (p.PoLot != null &&
                                p.PoLot.Length >= 5 &&
                                p.PoLot.Substring(0, 4) == targetDDMM &&
                                p.PoLot.Contains($"-{mcNo}-"))
                           ))
                    .ToListAsync();

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
                        scrapCount = 0,
                        message = $"ไม่พบข้อมูล LOT ของ MC {mcNo} ในวันที่ {targetDate:yyyy-MM-dd} (DDMM: {targetDDMM})"
                    });
                }

                int totalCount = records.Count;
                int okCount = records.Count(r => r.CheckSt);
                int ngCount = records.Count(r => !r.CheckSt && r.StatusTn?.ToUpper() != "SCRAP");
                int scrapCount = records.Count(r => r.StatusTn?.ToUpper() == "SCRAP");

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
                        NumberMatch = System.Text.RegularExpressions.Regex.Match(x.LastPart, @"[A-Z]?(\d+)"),
                        Letter = System.Text.RegularExpressions.Regex.Match(x.LastPart, @"-([A-Z]+)$").Groups[1].Value
                    })
                    .Select(x => new
                    {
                        x.Record,
                        Number = x.NumberMatch.Success && int.TryParse(x.NumberMatch.Groups[1].Value, out int num) ? num : 0,
                        x.Letter
                    })
                    .OrderBy(x => x.Number)
                    .ThenBy(x => x.Letter)
                    .Take(8)
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
                        check = r.StatusTn?.ToUpper() == "SCRAP" ? "SCRAP" : (r.CheckSt ? "OK" : "NG"),
                        checkDate = r.CheckDate,
                        mcNo = r.McNo,
                        lotQty = r.LotQty
                    }).ToList(),
                    totalCount = totalCount,
                    displayCount = sortedRecords.Count,
                    okCount = okCount,
                    ngCount = ngCount,
                    scrapCount = scrapCount,
                    date = targetDate.ToString("yyyy-MM-dd"),
                    targetDDMM = targetDDMM
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการดึงข้อมูล", error = ex.Message });
            }
        }

        [HttpGet("get-mc-list")]
        public async Task<IActionResult> GetMcList([FromQuery] string? date)
        {
            try
            {
                DateTime targetDate;

                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                }
                else
                {
                    targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                }

                // ✅ PoLot format: DDMMO - กรองตาม DDMM (วันที่เฉพาะ)
                string targetDDMM = $"{targetDate.Day:D2}{targetDate.Month:D2}";

                var mcList = await _context.PoCheckFlows
                    .Where(p =>
                        (p.CheckDate.HasValue && p.CheckDate.Value.Date == targetDate.Date)
                        ||
                        (p.PoLot != null && p.PoLot.Length >= 4 && p.PoLot.Substring(0, 4) == targetDDMM))
                    .Select(p => p.McNo)
                    .Distinct()
                    .OrderBy(mc => mc)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = mcList,
                    date = targetDate.ToString("yyyy-MM-dd"),
                    targetDDMM = targetDDMM,
                    count = mcList.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการดึงรายการ MC", error = ex.Message });
            }
        }

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

        [HttpGet("get-daily-stats")]
        public async Task<IActionResult> GetDailyStats([FromQuery] string mcNo, [FromQuery] DateTime? date)
        {
            try
            {
                var targetDate = date?.Date ?? DateTime.UtcNow.Date;

                // ✅ PoLot format: DDMMO - กรองตาม DDMM (วันที่เฉพาะ)
                string targetDDMM = $"{targetDate.Day:D2}{targetDate.Month:D2}";

                var query = _context.PoCheckFlows.AsQueryable();

                if (!string.IsNullOrEmpty(mcNo))
                {
                    query = query.Where(p => p.McNo == mcNo &&
                                             (
                                                 (p.CheckDate.HasValue && p.CheckDate.Value.Date == targetDate.Date)
                                                 ||
                                                 (p.PoLot != null && p.PoLot.Length >= 4 &&
                                                  p.PoLot.Substring(0, 4) == targetDDMM &&
                                                  p.PoLot.Contains($"-{mcNo}-"))
                                             ));
                }
                else
                {
                    query = query.Where(p =>
                        (p.CheckDate.HasValue && p.CheckDate.Value.Date == targetDate.Date)
                        ||
                        (p.PoLot != null && p.PoLot.Length >= 4 && p.PoLot.Substring(0, 4) == targetDDMM));
                }

                var records = await query.ToListAsync();
                var totalQty = records.Sum(r => r.LotQty ?? 0);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        date = targetDate,
                        targetDDMM = targetDDMM,
                        mcNo = mcNo,
                        totalLots = records.Count,
                        okCount = records.Count(r => r.CheckSt),
                        ngCount = records.Count(r => !r.CheckSt && r.StatusTn?.ToUpper() != "SCRAP"),
                        scrapCount = records.Count(r => r.StatusTn?.ToUpper() == "SCRAP"),
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

        [HttpGet("get-lots-with-quantity")]
        public async Task<IActionResult> GetLotsWithQuantity([FromQuery] string? mcNo, [FromQuery] DateTime? date)
        {
            try
            {
                var targetDate = date?.Date ?? DateTime.UtcNow.Date;

                var thRecordsQuery = _thicknessContext.ThRecords.AsQueryable();

                if (!string.IsNullOrEmpty(mcNo))
                {
                    thRecordsQuery = thRecordsQuery.Where(t => t.McPo == mcNo);
                }

                var thRecords = await thRecordsQuery.Where(x => x.DateProcess.Date == date.Value.Date).ToListAsync();

                var result = new List<object>();

                foreach (var thRecord in thRecords)
                {
                    string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";
                    string status = thRecord.Status ?? "";
                    bool checkSt = false;
                    int quantity = 0;

                    if (status.ToLower() == "rescreen")
                    {
                        var th100Record = await FindTH100Record(thRecord);

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
                        check = status.ToUpper() == "SCRAP" ? "SCRAP" : (checkSt ? "OK" : "NG"),
                        checkSt = checkSt,
                        quantity = quantity
                    });
                }

                int totalLots = result.Count;
                int okCount = result.Count(r => ((dynamic)r).checkSt == true);
                int ngCount = result.Count(r => ((dynamic)r).checkSt == false && ((dynamic)r).status?.ToUpper() != "SCRAP");
                int scrapCount = result.Count(r => ((dynamic)r).status?.ToUpper() == "SCRAP");
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
                        scrapCount = scrapCount,
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

                    if (status.ToLower() == "rescreen")
                    {
                        var th100Record = await FindTH100Record(thRecord);

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
                        check = status.ToUpper() == "SCRAP" ? "SCRAP" : (checkSt ? "OK" : "NG"),
                        checkSt = checkSt,
                        quantity = quantity
                    });
                }

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

                int okCount = sortedLots.Count(r => ((dynamic)r).checkSt == true);
                int ngCount = sortedLots.Count(r => ((dynamic)r).checkSt == false && ((dynamic)r).status?.ToUpper() != "SCRAP");
                int scrapCount = sortedLots.Count(r => ((dynamic)r).status?.ToUpper() == "SCRAP");
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
                        scrapCount = scrapCount,
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

        [HttpGet("get-mc-list-from-threcord")]
        public async Task<IActionResult> GetMcListFromThRecord([FromQuery] string? date)
        {
            try
            {
                DateTime targetDate;

                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                }
                else
                {
                    targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                }

                // ✅ PoLot format: DDMMO - กรองตาม DDMM (วันที่เฉพาะ)
                string targetDDMM = $"{targetDate.Day:D2}{targetDate.Month:D2}";

                var mcList = await _context.PoCheckFlows
                    .Where(p =>
                        (p.CheckDate.HasValue && p.CheckDate.Value.Date == targetDate.Date)
                        ||
                        (p.PoLot != null && p.PoLot.Length >= 4 && p.PoLot.Substring(0, 4) == targetDDMM))
                    .Select(p => p.McNo)
                    .Distinct()
                    .OrderBy(mc => mc)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = mcList,
                    date = targetDate.ToString("yyyy-MM-dd"),
                    targetDDMM = targetDDMM,
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

        [HttpGet("get-all-mc-from-threcord")]
        public async Task<IActionResult> GetAllMcFromThRecord()
        {
            try
            {
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

        [HttpGet("get-lots-summary-by-mc")]
        public async Task<IActionResult> GetLotsSummaryByMc([FromQuery] DateTime? date)
        {
            try
            {
                var targetDate = date?.Date ?? DateTime.UtcNow.Date;

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

                    if (status.ToLower() == "rescreen")
                    {
                        var th100Record = await FindTH100Record(thRecord);

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
                        check = status.ToUpper() == "SCRAP" ? "SCRAP" : (checkSt ? "OK" : "NG"),
                        checkSt = checkSt,
                        quantity = quantity
                    });

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
                                scrapCount = 0,
                                totalQty = 0
                            };
                        }

                        var current = mcSummary[thRecord.McPo];
                        bool isScrap = status.ToUpper() == "SCRAP";

                        mcSummary[thRecord.McPo] = new
                        {
                            mcNo = thRecord.McPo,
                            totalLots = current.totalLots + 1,
                            okCount = current.okCount + (checkSt && !isScrap ? 1 : 0),
                            ngCount = current.ngCount + (!checkSt && !isScrap ? 1 : 0),
                            scrapCount = current.scrapCount + (isScrap ? 1 : 0),
                            totalQty = current.totalQty + quantity
                        };
                    }
                }

                int totalLots = result.Count;
                int okCount = result.Count(r => ((dynamic)r).checkSt == true && ((dynamic)r).status?.ToUpper() != "SCRAP");
                int ngCount = result.Count(r => ((dynamic)r).checkSt == false && ((dynamic)r).status?.ToUpper() != "SCRAP");
                int scrapCount = result.Count(r => ((dynamic)r).status?.ToUpper() == "SCRAP");
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
                        scrapCount = scrapCount,
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

                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                }
                else
                {
                    targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                }

                // ✅ PoLot format: DDMMO - กรองตาม DDMM (วันที่เฉพาะ)
                string targetDDMM = $"{targetDate.Day:D2}{targetDate.Month:D2}";

                var records = await _context.PoCheckFlows
                    .Where(p => p.McNo == mcNo &&
                           (
                               (p.CheckDate.HasValue && p.CheckDate.Value.Date == targetDate.Date)
                               ||
                               (p.PoLot != null && p.PoLot.Length >= 4 &&
                                p.PoLot.Substring(0, 4) == targetDDMM &&
                                p.PoLot.Contains($"-{mcNo}-"))
                           ))
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
                    check = r.StatusTn?.ToUpper() == "SCRAP" ? "SCRAP" : (r.CheckSt ? "OK" : "NG"),
                    checkSt = r.CheckSt,
                    quantity = r.LotQty,
                    lotQty = r.LotQty,
                    checkDate = r.CheckDate
                }).ToList();

                int okCount = lotList.Count(r => r.checkSt && r.statusTn?.ToUpper() != "SCRAP");
                int ngCount = lotList.Count(r => !r.checkSt && r.statusTn?.ToUpper() != "SCRAP");
                int scrapCount = lotList.Count(r => r.statusTn?.ToUpper() == "SCRAP");
                int totalQty = lotList.Sum(r => r.quantity ?? 0);

                return Ok(new
                {
                    success = true,
                    data = lotList,
                    summary = new
                    {
                        mcNo = mcNo,
                        date = targetDate.ToString("yyyy-MM-dd"),
                        targetDDMM = targetDDMM,
                        totalLots = lotList.Count,
                        okCount = okCount,
                        ngCount = ngCount,
                        scrapCount = scrapCount,
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

                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                }
                else
                {
                    targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                }

                // ✅ PoLot format: DDMMO - กรองตาม DDMM (วันที่เฉพาะ)
                string targetDDMM = $"{targetDate.Day:D2}{targetDate.Month:D2}";

                var records = await _context.PoCheckFlows
                    .Where(p =>
                        (p.CheckDate.HasValue && p.CheckDate.Value.Date == targetDate.Date)
                        ||
                        (p.PoLot != null && p.PoLot.Length >= 4 && p.PoLot.Substring(0, 4) == targetDDMM))
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
                    check = r.StatusTn?.ToUpper() == "SCRAP" ? "SCRAP" : (r.CheckSt ? "OK" : "NG"),
                    checkSt = r.CheckSt,
                    quantity = r.LotQty,
                    lotQty = r.LotQty,
                    checkDate = r.CheckDate
                }).ToList();

                int okCount = lotList.Count(r => r.checkSt && r.statusTn?.ToUpper() != "SCRAP");
                int ngCount = lotList.Count(r => !r.checkSt && r.statusTn?.ToUpper() != "SCRAP");
                int scrapCount = lotList.Count(r => r.statusTn?.ToUpper() == "SCRAP");
                int totalQty = lotList.Sum(r => r.quantity ?? 0);

                return Ok(new
                {
                    success = true,
                    data = lotList,
                    summary = new
                    {
                        date = targetDate.ToString("yyyy-MM-dd"),
                        targetDDMM = targetDDMM,
                        totalLots = lotList.Count,
                        okCount = okCount,
                        ngCount = ngCount,
                        scrapCount = scrapCount,
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
}