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

                if (!string.IsNullOrEmpty(thRecord.Status))
                {
                    var statusLower = thRecord.Status.ToLower();

                    if (statusLower == "hold")
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "❌ LOT นี้อยู่ในสถานะ HOLD\n\n⚠️ ไม่สามารถบันทึกเข้าระบบได้",
                            status = "HOLD",
                            canSave = false
                        });
                    }

                    if (statusLower == "scrap")
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "❌ LOT นี้อยู่ในสถานะ SCRAP\n\n⚠️ ไม่สามารถบันทึกเข้าระบบได้",
                            status = "SCRAP",
                            canSave = false
                        });
                    }
                }

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
                    return BadRequest(new { success = false, message = $"รูปแบบ NO PO ไม่ถูกต้อง" });
                }

                string currentPoLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";
                string statusInfo = !string.IsNullOrEmpty(thRecord.Status) ? $"\n📊 Status: {thRecord.Status.ToUpper()}" : "";

                var autoAddedScrapLots = new List<string>();
                var rescreenSkippedLots = new List<string>();
                var rescreenCanAddLots = new List<string>();

                if (!existingNoPos.Any())
                {
                    if (currentNoPo > 1)
                    {
                        var missingNormalLots = new List<int>();

                        for (int i = 1; i < currentNoPo; i++)
                        {
                            var lotStatus = await CheckLotStatusEnhanced(thRecord.LotPo, thRecord.McPo, i);

                            if (lotStatus.isScrap && lotStatus.thRecord != null)
                            {
                                await AutoAddScrapLot(lotStatus.thRecord);
                                autoAddedScrapLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{lotStatus.thRecord.NoPo}");
                            }
                            else if (lotStatus.isRescreen)
                            {
                                var (canSkip, canAdd, source) = await CheckRescreenLotStatus(thRecord.LotPo, thRecord.McPo, i);

                                if (canSkip)
                                {
                                    rescreenSkippedLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{i:D3}");
                                }
                                else if (canAdd)
                                {
                                    rescreenCanAddLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{i:D3}");
                                    missingNormalLots.Add(i);
                                }
                                else
                                {
                                    missingNormalLots.Add(i);
                                }
                            }
                            else if (!lotStatus.isHold)
                            {
                                missingNormalLots.Add(i);
                            }
                        }

                        if (missingNormalLots.Any())
                        {
                            var errorMessage = $"❌ ไม่มี LOT ของ {thRecord.LotPo}-{thRecord.McPo} ในระบบ\n\n📋 กรุณาเริ่มต้นด้วย: {thRecord.LotPo}-{thRecord.McPo}-001\n\n🔍 LOT ที่พยายามเพิ่ม: {currentPoLot}{statusInfo}";

                            if (rescreenSkippedLots.Any())
                            {
                                errorMessage += $"\n\n✅ LOT ที่ข้ามได้ (มีใน Rescreen Check):\n• {string.Join("\n• ", rescreenSkippedLots)}";
                            }

                            if (rescreenCanAddLots.Any())
                            {
                                errorMessage += $"\n\n📌 LOT Rescreen ที่ต้องเพิ่มก่อน (มีใน TH100):\n• {string.Join("\n• ", rescreenCanAddLots)}";
                            }

                            return BadRequest(new
                            {
                                success = false,
                                message = errorMessage,
                                requiredLot = $"{thRecord.LotPo}-{thRecord.McPo}-001",
                                currentLot = currentPoLot,
                                status = thRecord.Status,
                                rescreenSkippedLots = rescreenSkippedLots,
                                rescreenCanAddLots = rescreenCanAddLots
                            });
                        }

                        if (autoAddedScrapLots.Any())
                        {
                            statusInfo += $"\n\n✅ Auto-add SCRAP LOT:\n• {string.Join("\n• ", autoAddedScrapLots)}";
                        }

                        if (rescreenSkippedLots.Any())
                        {
                            statusInfo += $"\n\n✅ ข้าม Rescreen LOT (มีใน Rescreen Check):\n• {string.Join("\n• ", rescreenSkippedLots)}";
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
                            var lotStatus = await CheckLotStatusEnhanced(thRecord.LotPo, thRecord.McPo, i);

                            if (lotStatus.isScrap && lotStatus.thRecord != null)
                            {
                                await AutoAddScrapLot(lotStatus.thRecord);
                                autoAddedScrapLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{lotStatus.thRecord.NoPo}");
                            }
                            else if (lotStatus.isRescreen)
                            {
                                var (canSkip, canAdd, source) = await CheckRescreenLotStatus(thRecord.LotPo, thRecord.McPo, i);

                                if (canSkip)
                                {
                                    rescreenSkippedLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{i:D3}");
                                }
                                else if (canAdd)
                                {
                                    rescreenCanAddLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{i:D3}");
                                    missingNormalLots.Add(i);
                                }
                                else
                                {
                                    missingNormalLots.Add(i);
                                }
                            }
                            else if (!lotStatus.isHold)
                            {
                                missingNormalLots.Add(i);
                            }
                        }
                    }

                    if (missingNormalLots.Any())
                    {
                        var missingLotsList = string.Join("\n• ", missingNormalLots.Select(n => $"{thRecord.LotPo}-{thRecord.McPo}-{n:D3}"));
                        var errorMessage = $"❌ ไม่สามารถเพิ่ม LOT นี้ได้\n\n⚠️ ยังมี LOT ก่อนหน้าที่ยังไม่ได้เพิ่ม:\n• {missingLotsList}\n\n🔍 LOT ที่พยายามเพิ่ม: {currentPoLot}{statusInfo}";

                        if (rescreenSkippedLots.Any())
                        {
                            errorMessage += $"\n\n✅ LOT ที่ข้ามได้ (มีใน Rescreen Check):\n• {string.Join("\n• ", rescreenSkippedLots)}";
                        }

                        if (rescreenCanAddLots.Any())
                        {
                            errorMessage += $"\n\n📌 LOT Rescreen ที่ต้องเพิ่มก่อน (มีใน TH100):\n• {string.Join("\n• ", rescreenCanAddLots)}";
                        }

                        return BadRequest(new
                        {
                            success = false,
                            message = errorMessage,
                            missingLots = missingNormalLots.Select(n => $"{thRecord.LotPo}-{thRecord.McPo}-{n:D3}").ToList(),
                            rescreenSkippedLots = rescreenSkippedLots,
                            rescreenCanAddLots = rescreenCanAddLots,
                            currentLot = currentPoLot,
                            status = thRecord.Status
                        });
                    }

                    if (autoAddedScrapLots.Any())
                    {
                        statusInfo += $"\n\n✅ Auto-add SCRAP LOT:\n• {string.Join("\n• ", autoAddedScrapLots)}";
                    }

                    if (rescreenSkippedLots.Any())
                    {
                        statusInfo += $"\n\n✅ ข้าม Rescreen LOT (มีใน Rescreen Check):\n• {string.Join("\n• ", rescreenSkippedLots)}";
                    }
                }

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
                    .FirstOrDefaultAsync(p => p.Imobilelot == thRecord.ImobileLot);

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
                        rescreenSkippedLots = rescreenSkippedLots,
                        rescreenCanAddLots = rescreenCanAddLots
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาด", error = ex.Message });
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
                    return BadRequest(new { success = false, message = $"รูปแบบ NO PO ไม่ถูกต้อง" });
                }

                var autoAddedScrapLots = new List<string>();
                var rescreenSkippedLots = new List<string>();
                var rescreenCanAddLots = new List<string>();

                if (!existingNoPos.Any())
                {
                    if (currentNoPo > 1)
                    {
                        var missingNormalLots = new List<int>();

                        for (int i = 1; i < currentNoPo; i++)
                        {
                            var lotStatus = await CheckLotStatusEnhanced(thRecord.LotPo, thRecord.McPo, i);

                            if (lotStatus.isScrap && lotStatus.thRecord != null)
                            {
                                await AutoAddScrapLot(lotStatus.thRecord);
                                autoAddedScrapLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{lotStatus.thRecord.NoPo}");
                            }
                            else if (lotStatus.isRescreen)
                            {
                                var (canSkip, canAdd, source) = await CheckRescreenLotStatus(thRecord.LotPo, thRecord.McPo, i);

                                if (canSkip)
                                {
                                    rescreenSkippedLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{i:D3}");
                                }
                                else if (canAdd)
                                {
                                    rescreenCanAddLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{i:D3}");
                                    missingNormalLots.Add(i);
                                }
                                else
                                {
                                    missingNormalLots.Add(i);
                                }
                            }
                            else if (!lotStatus.isHold)
                            {
                                missingNormalLots.Add(i);
                            }
                        }

                        if (missingNormalLots.Any())
                        {
                            var validationMessage = $"ไม่มี LOT ของ {thRecord.LotPo}-{thRecord.McPo} ในระบบ กรุณาเริ่มต้นด้วย {thRecord.LotPo}-{thRecord.McPo}-001";

                            if (rescreenSkippedLots.Any())
                            {
                                validationMessage += $"\n\n✅ LOT ที่ข้ามได้ (มีใน Rescreen Check): {string.Join(", ", rescreenSkippedLots)}";
                            }

                            if (rescreenCanAddLots.Any())
                            {
                                validationMessage += $"\n\n📌 LOT Rescreen ที่ต้องเพิ่มก่อน (มีใน TH100): {string.Join(", ", rescreenCanAddLots)}";
                            }

                            return BadRequest(new
                            {
                                success = false,
                                message = validationMessage
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
                            var lotStatus = await CheckLotStatusEnhanced(thRecord.LotPo, thRecord.McPo, i);

                            if (lotStatus.isScrap && lotStatus.thRecord != null)
                            {
                                await AutoAddScrapLot(lotStatus.thRecord);
                                autoAddedScrapLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{lotStatus.thRecord.NoPo}");
                            }
                            else if (lotStatus.isRescreen)
                            {
                                var (canSkip, canAdd, source) = await CheckRescreenLotStatus(thRecord.LotPo, thRecord.McPo, i);

                                if (canSkip)
                                {
                                    rescreenSkippedLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{i:D3}");
                                }
                                else if (canAdd)
                                {
                                    rescreenCanAddLots.Add($"{thRecord.LotPo}-{thRecord.McPo}-{i:D3}");
                                    missingNormalLots.Add(i);
                                }
                                else
                                {
                                    missingNormalLots.Add(i);
                                }
                            }
                            else if (!lotStatus.isHold)
                            {
                                missingNormalLots.Add(i);
                            }
                        }
                    }

                    if (missingNormalLots.Any())
                    {
                        var validationMessage = $"ไม่สามารถเพิ่ม LOT นี้ได้ เนื่องจากยังมี LOT ก่อนหน้าที่ยังไม่ได้เพิ่ม: {string.Join(", ", missingNormalLots.Select(n => $"{thRecord.LotPo}-{thRecord.McPo}-{n:D3}"))}";

                        if (rescreenSkippedLots.Any())
                        {
                            validationMessage += $"\n\n✅ LOT ที่ข้ามได้ (มีใน Rescreen Check): {string.Join(", ", rescreenSkippedLots)}";
                        }

                        if (rescreenCanAddLots.Any())
                        {
                            validationMessage += $"\n\n📌 LOT Rescreen ที่ต้องเพิ่มก่อน (มีใน TH100): {string.Join(", ", rescreenCanAddLots)}";
                        }

                        return BadRequest(new
                        {
                            success = false,
                            message = validationMessage
                        });
                    }
                }

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

                var successMessage = $"✅ บันทึก LOT {poLot} สำเร็จ";

                if (autoAddedScrapLots.Any())
                {
                    successMessage += $"\n\n🔄 Auto-add SCRAP LOT:\n• {string.Join("\n• ", autoAddedScrapLots)}";
                }

                if (rescreenSkippedLots.Any())
                {
                    successMessage += $"\n\n✅ ข้าม Rescreen LOT (มีใน Rescreen Check):\n• {string.Join("\n• ", rescreenSkippedLots)}";
                }

                return Ok(new
                {
                    success = true,
                    message = successMessage,
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
                        autoAddedScrapLots = autoAddedScrapLots,
                        rescreenSkippedLots = rescreenSkippedLots,
                        rescreenCanAddLots = rescreenCanAddLots
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการบันทึก", error = ex.Message });
            }
        }

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

                string targetDDMM = $"{targetDate.Day:D2}{targetDate.Month:D2}";

                var records = await _context.PoCheckFlows
                    .Where(p => p.McNo == mcNo &&
                           (
                               (p.CheckDate.HasValue &&
                                p.CheckDate.Value.Year == targetDate.Year &&
                                p.CheckDate.Value.Month == targetDate.Month &&
                                p.CheckDate.Value.Day == targetDate.Day)
                               ||
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
                    return NotFound(new { success = false, message = "ไม่พบข้อมูล LOT" });
                }

                _context.PoCheckFlows.Remove(record);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "ลบข้อมูล LOT สำเร็จ" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการลบข้อมูล", error = ex.Message });
            }
        }

        [HttpGet("get-daily-stats")]
        public async Task<IActionResult> GetDailyStats([FromQuery] string mcNo, [FromQuery] DateTime? date)
        {
            try
            {
                var targetDate = date?.Date ?? DateTime.UtcNow.Date;
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
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการดึงสถิติ", error = ex.Message });
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

                return Ok(new { success = true, data = records });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการดึงข้อมูล", error = ex.Message });
            }
        }
        [HttpGet("get-all-lots-by-date")]
        public async Task<IActionResult> GetAllLotsByDate([FromQuery] string? date)
        {
            try
            {
                DateTime targetDate;
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                    targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                else
                    targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

                string targetDDMM = $"{targetDate.Day:D2}{targetDate.Month:D2}";

                var records = await _context.PoCheckFlows
                    .Where(p =>
                        (p.CheckDate.HasValue &&
                         p.CheckDate.Value.Year == targetDate.Year &&
                         p.CheckDate.Value.Month == targetDate.Month &&
                         p.CheckDate.Value.Day == targetDate.Day)
                        ||
                        (p.PoLot != null && p.PoLot.Length >= 4 &&
                         p.PoLot.Substring(0, 4) == targetDDMM))
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
                        check = r.StatusTn?.ToUpper() == "SCRAP" ? "SCRAP" : (r.CheckSt ? "OK" : "NG"),
                        checkDate = r.CheckDate,
                        mcNo = r.McNo,
                        lotQty = r.LotQty
                    }).ToList(),
                    date = targetDate.ToString("yyyy-MM-dd"),
                    targetDDMM = targetDDMM,
                    count = records.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error", error = ex.Message });
            }
        }
        private async Task<Th100Record?> FindTH100Record(ThRecord thRecord)
        {
            var noPoNumber = System.Text.RegularExpressions.Regex.Match(thRecord.NoPo ?? "", @"^(\d+)").Groups[1].Value;

            if (string.IsNullOrEmpty(noPoNumber))
                return null;

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

            return await _thicknessContext.Th100Records
                .FirstOrDefaultAsync(t => t.ImobileLot == thRecord.ImobileLot);
        }

        private async Task AutoAddScrapLot(ThRecord thRecord)
        {
            if (thRecord == null) return;

            string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";

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
                    LotQty = 0
                };

                _context.PoCheckFlows.Add(scrapLot);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<(bool isScrap, bool isHold, string status, ThRecord thRecord)> CheckLotStatus(string lotPo, string mcPo, int noPoNumber)
        {
            var result = await CheckLotStatusEnhanced(lotPo, mcPo, noPoNumber);
            return (result.isScrap, result.isHold, result.status, result.thRecord);
        }

        private async Task<(bool exists, bool isOK, string status)> CheckRescreenRecord(string lotPo, string mcPo, int noPoNumber)
        {
            var rescreenRecords = await _context.RescreenCheckRecords1
                .Where(r => r.LotPo == lotPo && r.McPo == mcPo)
                .ToListAsync();

            foreach (var record in rescreenRecords)
            {
                var recordNoPoNumber = System.Text.RegularExpressions.Regex.Match(record.NoPo ?? "", @"^(\d+)").Groups[1].Value;

                if (int.TryParse(recordNoPoNumber, out int num) && num == noPoNumber)
                {
                    if (!string.IsNullOrEmpty(record.FinalStatus))
                    {
                        var statusLower = record.FinalStatus.ToLower().Trim();

                        if (statusLower == "ok" || statusLower.Contains("ok"))
                        {
                            return (true, true, record.FinalStatus);
                        }
                        else
                        {
                            return (true, false, record.FinalStatus);
                        }
                    }

                    return (true, false, "No Status");
                }
            }

            return (false, false, "");
        }

        private async Task<bool> CheckTH100Record(string lotPo, string mcPo, int noPoNumber)
        {
            var th100Records = await _thicknessContext.Th100Records
                .Where(t => t.LotPo == lotPo && t.McPo == mcPo)
                .ToListAsync();

            foreach (var th100 in th100Records)
            {
                var th100NoPoNumber = System.Text.RegularExpressions.Regex.Match(th100.NoPo ?? "", @"^(\d+)").Groups[1].Value;

                if (int.TryParse(th100NoPoNumber, out int num) && num == noPoNumber)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<(bool canSkip, bool canAdd, string source)> CheckRescreenLotStatus(string lotPo, string mcPo, int noPoNumber)
        {
            var (exists, isOK, status) = await CheckRescreenRecord(lotPo, mcPo, noPoNumber);

            if (exists && isOK)
            {
                return (canSkip: true, canAdd: false, source: "rescreen_check_record");
            }

            var hasTH100 = await CheckTH100Record(lotPo, mcPo, noPoNumber);
            if (hasTH100)
            {
                return (canSkip: false, canAdd: true, source: "th100_record");
            }

            return (canSkip: false, canAdd: false, source: "none");
        }

        private async Task<(bool isScrap, bool isHold, bool isRescreen, string status, ThRecord thRecord)> CheckLotStatusEnhanced(string lotPo, string mcPo, int noPoNumber)
        {
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
                            return (true, false, false, thRecord.Status, thRecord);
                        }

                        if (statusLower == "hold")
                        {
                            return (false, true, false, thRecord.Status, thRecord);
                        }

                        if (statusLower == "rescreen")
                        {
                            return (false, false, true, thRecord.Status, thRecord);
                        }
                    }

                    return (false, false, false, thRecord.Status ?? "", thRecord);
                }
            }

            return (false, false, false, "", null);
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