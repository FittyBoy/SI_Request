using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SI24004.Models.PostgreSQL;
using SI24004.Models.SqlServer;
using SI24004.Models.SqlServer1;

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

        // ─────────────────────────────────────────────
        // REP Product Helpers
        // ─────────────────────────────────────────────

        private bool IsRepLot(string lotNumber)
        {
            if (string.IsNullOrEmpty(lotNumber)) return false;
            var parts = lotNumber.Split('-');
            return parts.Length >= 3 && parts[1].ToUpper() == "REP";
        }

        private (string prefix, int sequence, string suffix) ParseRepLot(string lotNumber)
        {
            var parts = lotNumber.Split('-');
            // format: 0403O-REP-001-N
            string prefix = $"{parts[0]}-{parts[1]}";
            int.TryParse(parts[2], out int seq);
            string suffix = parts.Length >= 4 ? parts[3] : "";
            return (prefix, seq, suffix);
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

                // ✅ REP Product — bypass ThRecord lookup entirely
                if (IsRepLot(request.LotNumber))
                {
                    var (prefix, currentSeq, suffix) = ParseRepLot(request.LotNumber);

                    var existingRepLots = await _context.PoCheckFlows
                        .Where(p => p.McNo == "REP" && p.PoLot != null && p.PoLot.StartsWith(prefix + "-"))
                        .ToListAsync();

                    var existingSeqs = existingRepLots
                        .Select(p => {
                            var pts = p.PoLot!.Split('-');
                            return pts.Length >= 3 && int.TryParse(pts[2], out int n) ? n : -1;
                        })
                        .Where(n => n > 0)
                        .ToHashSet();

                    var missingSeqs = new List<int>();
                    for (int i = 1; i < currentSeq; i++)
                    {
                        if (!existingSeqs.Contains(i))
                            missingSeqs.Add(i);
                    }

                    if (missingSeqs.Any())
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"❌ ไม่สามารถเพิ่ม LOT นี้ได้\n\n⚠️ ยังมี LOT ก่อนหน้าที่ยังไม่ได้เพิ่ม",
                            missingLots = missingSeqs.Select(n => $"{prefix}-{n:D3}-{suffix}").ToList(),
                            scrapHoldLots = new List<string>(),
                            currentLot = request.LotNumber
                        });
                    }

                    var existingRep = await _context.PoCheckFlows
                        .FirstOrDefaultAsync(p => p.PoLot == request.LotNumber && p.McNo == "REP");

                    return Ok(new
                    {
                        success = true,
                        message = "✅ REP Product — พร้อมบันทึก",
                        data = new
                        {
                            imobileLot = (string?)null,
                            poLot = request.LotNumber,
                            statusTn = "OK",
                            checkSt = true,
                            mcNo = "REP",
                            isRepProduct = true,
                            isDuplicate = existingRep != null,
                            existingQty = (int?)null,
                            // ✅ REP ไม่มี cassetteNo
                            cassetteNo = (string?)null
                        }
                    });
                }

                // ✅ ดึง ThRecord ทั้งหมดที่ ImobileLot ตรงกัน
                var allThRecords = await _thicknessContext.ThRecords
                    .Where(t => t.ImobileLot == request.LotNumber)
                    .OrderBy(t => t.TimeProcess)
                    .ToListAsync();

                if (!allThRecords.Any())
                {
                    return NotFound(new { success = false, message = $"ไม่พบข้อมูล ImobileLot: {request.LotNumber}" });
                }

                // ✅ BR29x30x0.6: เลือก ThRecord ที่ Process = Colloidal ก่อนเสมอ
                // และข้าม sequence check ของ lot ที่มี ImobileLot เดียวกันได้
                bool isBrSize = allThRecords.Any(t =>
                    t.ImobileSize != null &&
                    t.ImobileSize.Equals("BR29x30x0.6", StringComparison.OrdinalIgnoreCase));

                ThRecord thRecord = null;

                if (isBrSize)
                {
                    // หา Colloidal ที่ยังไม่ถูกบันทึกก่อน
                    var colloidalRecords = allThRecords
                        .Where(t => t.Process != null &&
                                    t.Process.Equals("Colloidal", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(t => t.TimeProcess);

                    foreach (var tr in colloidalRecords)
                    {
                        string candidatePoLot = $"{tr.LotPo}-{tr.McPo}-{tr.NoPo}";
                        var existsInFlow = await _context.PoCheckFlows
                            .AnyAsync(p => p.PoLot == candidatePoLot && p.McNo == tr.McPo);
                        if (!existsInFlow)
                        {
                            thRecord = tr;
                            break;
                        }
                    }

                    // ถ้า Colloidal ถูกบันทึกหมดแล้ว หาตัวอื่นที่ยังไม่ถูกบันทึก
                    if (thRecord == null)
                    {
                        foreach (var tr in allThRecords.OrderByDescending(t => t.TimeProcess))
                        {
                            string candidatePoLot = $"{tr.LotPo}-{tr.McPo}-{tr.NoPo}";
                            var existsInFlow = await _context.PoCheckFlows
                                .AnyAsync(p => p.PoLot == candidatePoLot && p.McNo == tr.McPo);
                            if (!existsInFlow)
                            {
                                thRecord = tr;
                                break;
                            }
                        }
                    }

                    if (thRecord == null)
                    {
                        var latestTh = allThRecords.OrderByDescending(t => t.TimeProcess).First();
                        return BadRequest(new
                        {
                            success = false,
                            message = "❌ ImobileLot นี้ถูกบันทึกครบทุก NoPo แล้ว",
                            status = "DUPLICATE",
                            currentLot = $"{latestTh.LotPo}-{latestTh.McPo}-{latestTh.NoPo}"
                        });
                    }
                }
                else
                {
                    // ปกติ: หา ThRecord ที่ยังไม่มีใน po_check_flow
                    foreach (var tr in allThRecords.OrderByDescending(t => t.TimeProcess))
                    {
                        string candidatePoLot = $"{tr.LotPo}-{tr.McPo}-{tr.NoPo}";
                        var existsInFlow = await _context.PoCheckFlows
                            .AnyAsync(p => p.PoLot == candidatePoLot && p.McNo == tr.McPo);
                        if (!existsInFlow)
                        {
                            thRecord = tr;
                            break;
                        }
                    }

                    if (thRecord == null)
                    {
                        var latestTh = allThRecords.OrderByDescending(t => t.TimeProcess).First();
                        return BadRequest(new
                        {
                            success = false,
                            message = "❌ ImobileLot นี้ถูกบันทึกครบทุก NoPo แล้ว",
                            status = "DUPLICATE",
                            currentLot = $"{latestTh.LotPo}-{latestTh.McPo}-{latestTh.NoPo}"
                        });
                    }

                    // เช็คว่า NoPo ตัวเลขเดียวกันมีใน flow แล้วหรือไม่ (ไม่ filter McNo)
                    var currentNoPoNumMatch = System.Text.RegularExpressions.Regex.Match(thRecord.NoPo ?? "", @"^(\d+)");
                    if (currentNoPoNumMatch.Success)
                    {
                        string noPoNum = currentNoPoNumMatch.Groups[1].Value;
                        var sameSeqExists = await _context.PoCheckFlows
                            .AnyAsync(p => p.PoLot != null &&
                                           p.PoLot.StartsWith($"{thRecord.LotPo}-{thRecord.McPo}-") &&
                                           p.PoLot.Contains($"-{noPoNum.PadLeft(3, '0')}-"));
                        if (sameSeqExists)
                        {
                            string thisPoLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";
                            return BadRequest(new
                            {
                                success = false,
                                message = $"❌ LOT ลำดับที่ {noPoNum} เคยถูกบันทึกแล้ว ไม่สามารถเพิ่มซ้ำได้",
                                status = "DUPLICATE",
                                currentLot = thisPoLot
                            });
                        }
                    }
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
                bool checkSt = false;
                bool hasTH100 = false;
                string finalStatus = thRecord.Status;
                string th100Status = (string)null;
                // ✅ BR29x30x0.6: ข้าม sequence check ได้เลย
                if (isBrSize) goto SkipSequenceCheck;


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
                                if (lotStatus.thRecord == null)
                                {
                                    missingNormalLots.Add(i);
                                    continue;
                                }

                                var (canSkip, canAdd, source) = await CheckRescreenLotStatus(
                                    lotStatus.thRecord.ImobileLot,
                                    thRecord.LotPo,
                                    thRecord.McPo,
                                    i
                                );

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
                                if (lotStatus.thRecord == null)
                                {
                                    missingNormalLots.Add(i);
                                    continue;
                                }

                                var (canSkip, canAdd, source) = await CheckRescreenLotStatus(
                                    lotStatus.thRecord.ImobileLot,
                                    thRecord.LotPo,
                                    thRecord.McPo,
                                    i
                                );

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

            SkipSequenceCheck:
                hasTH100 = false;
                finalStatus = thRecord.Status;
                th100Status = null;

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

                // ✅ เช็ค duplicate ด้วย PoLot เป็น key หลัก
                var existingByPoLot = await _context.PoCheckFlows
                    .FirstOrDefaultAsync(p => p.PoLot == poLot && p.McNo == thRecord.McPo);

                bool isDuplicate = existingByPoLot != null;

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
                        isDuplicate = isDuplicate,
                        existingQty = existingByPoLot?.LotQty,
                        autoAddedScrapLots = autoAddedScrapLots,
                        rescreenSkippedLots = rescreenSkippedLots,
                        rescreenCanAddLots = rescreenCanAddLots,
                        cassetteNo = thRecord.Ca5In9
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
                // ✅ REP Product — บันทึกผ่าน PoLot โดยตรง
                if (!string.IsNullOrEmpty(request.PoLot) && IsRepLot(request.PoLot))
                {
                    var (prefix, currentSeq, suffix) = ParseRepLot(request.PoLot);

                    var existingRepLots = await _context.PoCheckFlows
                        .Where(p => p.McNo == "REP" && p.PoLot != null && p.PoLot.StartsWith(prefix + "-"))
                        .ToListAsync();

                    var existingSeqs = existingRepLots
                        .Select(p => {
                            var pts = p.PoLot!.Split('-');
                            return pts.Length >= 3 && int.TryParse(pts[2], out int n) ? n : -1;
                        })
                        .Where(n => n > 0)
                        .ToHashSet();

                    var missingSeqs = new List<int>();
                    for (int i = 1; i < currentSeq; i++)
                    {
                        if (!existingSeqs.Contains(i))
                            missingSeqs.Add(i);
                    }

                    if (missingSeqs.Any())
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"ไม่สามารถเพิ่ม LOT นี้ได้ เนื่องจากยังมี LOT ก่อนหน้าที่ยังไม่ได้เพิ่ม: {string.Join(", ", missingSeqs.Select(n => $"{prefix}-{n:D3}-{suffix}"))}"
                        });
                    }

                    var checkDate = DateOnly.FromDateTime(DateTime.UtcNow);

                    var existingRep = await _context.PoCheckFlows
                        .FirstOrDefaultAsync(p => p.PoLot == request.PoLot && p.McNo == "REP");

                    PoCheckFlow repFlow;

                    if (existingRep != null)
                    {
                        existingRep.CheckDate = checkDate;
                        existingRep.StatusTn = "OK";
                        existingRep.CheckSt = true;
                        repFlow = existingRep;
                    }
                    else
                    {
                        repFlow = new PoCheckFlow
                        {
                            PoLot = request.PoLot,
                            Imobilelot = null,
                            StatusTn = "OK",
                            CheckSt = true,
                            CheckDate = checkDate,
                            McNo = "REP",
                            LotQty = null
                        };
                        _context.PoCheckFlows.Add(repFlow);
                    }

                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = $"✅ บันทึก REP LOT {request.PoLot} สำเร็จ",
                        data = new
                        {
                            id = repFlow.Id,
                            poLot = repFlow.PoLot,
                            imobileLot = (string?)null,
                            statusTn = "OK",
                            checkSt = true,
                            check = "OK",
                            checkDate = repFlow.CheckDate,
                            mcNo = "REP",
                            lotQty = (int?)null
                        }
                    });
                }

                if (string.IsNullOrWhiteSpace(request.ImobileLot))
                {
                    return BadRequest(new { success = false, message = "กรุณาระบุ ImobileLot" });
                }

                // ✅ ดึง ThRecord ทั้งหมดที่ ImobileLot ตรงกัน หาตัวที่ยังไม่ถูกบันทึก
                var allThRecordsSave = await _thicknessContext.ThRecords
                    .Where(t => t.ImobileLot == request.ImobileLot)
                    .OrderBy(t => t.TimeProcess)
                    .ToListAsync();

                if (!allThRecordsSave.Any())
                {
                    return NotFound(new { success = false, message = "ไม่พบข้อมูล LOT" });
                }

                // ✅ BR29x30x0.6: เลือก Colloidal ก่อน และข้าม sequence check
                bool isBrSizeSave = allThRecordsSave.Any(t =>
                    t.ImobileSize != null &&
                    t.ImobileSize.Equals("BR29x30x0.6", StringComparison.OrdinalIgnoreCase));

                ThRecord thRecord = null;

                if (isBrSizeSave)
                {
                    // หา Colloidal ที่ยังไม่ถูกบันทึกก่อน
                    var colloidalSave = allThRecordsSave
                        .Where(t => t.Process != null &&
                                    t.Process.Equals("Colloidal", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(t => t.TimeProcess);

                    foreach (var tr in colloidalSave)
                    {
                        string candidatePoLot = $"{tr.LotPo}-{tr.McPo}-{tr.NoPo}";
                        var existsInFlow = await _context.PoCheckFlows
                            .AnyAsync(p => p.PoLot == candidatePoLot && p.McNo == tr.McPo);
                        if (!existsInFlow)
                        {
                            thRecord = tr;
                            break;
                        }
                    }

                    // ถ้า Colloidal หมดแล้ว หาตัวอื่น
                    if (thRecord == null)
                    {
                        foreach (var tr in allThRecordsSave.OrderByDescending(t => t.TimeProcess))
                        {
                            string candidatePoLot = $"{tr.LotPo}-{tr.McPo}-{tr.NoPo}";
                            var existsInFlow = await _context.PoCheckFlows
                                .AnyAsync(p => p.PoLot == candidatePoLot && p.McNo == tr.McPo);
                            if (!existsInFlow)
                            {
                                thRecord = tr;
                                break;
                            }
                        }
                    }

                    if (thRecord == null)
                    {
                        return BadRequest(new { success = false, message = "ImobileLot นี้ถูกบันทึกครบทุก NoPo แล้ว", status = "DUPLICATE" });
                    }
                }
                else
                {
                    foreach (var tr in allThRecordsSave.OrderByDescending(t => t.TimeProcess))
                    {
                        string candidatePoLot = $"{tr.LotPo}-{tr.McPo}-{tr.NoPo}";
                        var existsInFlow = await _context.PoCheckFlows
                            .AnyAsync(p => p.PoLot == candidatePoLot && p.McNo == tr.McPo);
                        if (!existsInFlow)
                        {
                            thRecord = tr;
                            break;
                        }
                    }

                    if (thRecord == null)
                    {
                        return BadRequest(new { success = false, message = "ImobileLot นี้ถูกบันทึกครบทุก NoPo แล้ว", status = "DUPLICATE" });
                    }

                    // เช็ค same-sequence duplicate (ไม่ filter McNo)
                    var saveNoPoNumMatch = System.Text.RegularExpressions.Regex.Match(thRecord.NoPo ?? "", @"^(\d+)");
                    if (saveNoPoNumMatch.Success)
                    {
                        string saveNoPoNum = saveNoPoNumMatch.Groups[1].Value;
                        var saveSameSeqExists = await _context.PoCheckFlows
                            .AnyAsync(p => p.PoLot != null &&
                                           p.PoLot.StartsWith($"{thRecord.LotPo}-{thRecord.McPo}-") &&
                                           p.PoLot.Contains($"-{saveNoPoNum.PadLeft(3, '0')}-"));
                        if (saveSameSeqExists)
                        {
                            return BadRequest(new
                            {
                                success = false,
                                message = $"LOT ลำดับที่ {saveNoPoNum} เคยถูกบันทึกแล้ว ไม่สามารถเพิ่มซ้ำได้",
                                status = "DUPLICATE"
                            });
                        }
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

                var autoAddedScrapLots = new List<string>();
                var rescreenSkippedLots = new List<string>();
                var rescreenCanAddLots = new List<string>();
                bool checkSt = false;
                string finalStatus = thRecord.Status;
                // ✅ BR29x30x0.6: ข้าม sequence check ได้เลย
                if (isBrSizeSave) goto SkipSequenceCheckSave;


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
                                if (lotStatus.thRecord == null)
                                {
                                    missingNormalLots.Add(i);
                                    continue;
                                }

                                var (canSkip, canAdd, source) = await CheckRescreenLotStatus(
                                    lotStatus.thRecord.ImobileLot,
                                    thRecord.LotPo,
                                    thRecord.McPo,
                                    i
                                );

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
                                if (lotStatus.thRecord == null)
                                {
                                    missingNormalLots.Add(i);
                                    continue;
                                }

                                var (canSkip, canAdd, source) = await CheckRescreenLotStatus(
                                    lotStatus.thRecord.ImobileLot,
                                    thRecord.LotPo,
                                    thRecord.McPo,
                                    i
                                );

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

            SkipSequenceCheckSave:
                finalStatus = thRecord.Status;

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

                // ✅ เช็คด้วย PoLot เป็น key หลัก (LotPo-McPo-NoPo)
                // - ถ้า PoLot ตรง → update (สแกนซ้ำ lot เดิม)
                // - ถ้า PoLot ต่าง (NoPo ต่าง) → create new record
                var existingByPoLot = await _context.PoCheckFlows
                    .FirstOrDefaultAsync(p => p.PoLot == poLot && p.McNo == thRecord.McPo);

                PoCheckFlow poCheckFlow;
                var checkDate2 = DateOnly.FromDateTime(DateTime.UtcNow);

                if (existingByPoLot != null)
                {
                    // PoLot นี้เคยบันทึกแล้ว → block ไม่ให้เพิ่มซ้ำ
                    return BadRequest(new
                    {
                        success = false,
                        message = $"LOT {poLot} เคยถูกบันทึกแล้ว ไม่สามารถเพิ่มซ้ำได้",
                        status = "DUPLICATE"
                    });
                }

                // PoLot ยังไม่มีใน DB → CREATE NEW
                poCheckFlow = new PoCheckFlow
                {
                    PoLot = poLot,
                    Imobilelot = thRecord.ImobileLot,
                    StatusTn = finalStatus,
                    CheckSt = checkSt,
                    CheckDate = checkDate2,
                    McNo = thRecord.McPo,
                    LotQty = totalQty
                };
                _context.PoCheckFlows.Add(poCheckFlow);

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
                        check = poCheckFlow.CheckSt == true ? "OK" : "NG",
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
                return StatusCode(500, new { 
                    success = false, 
                    message = "เกิดข้อผิดพลาดในการบันทึก", 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    detail = ex.InnerException?.InnerException?.Message
                });
            }
        }

        [HttpGet("get-lots-by-mc")]
        public async Task<IActionResult> GetLotsByMc([FromQuery] string mcNo, [FromQuery] string? date)
        {
            try
            {
                if (string.IsNullOrEmpty(mcNo))
                    return BadRequest(new { success = false, message = "กรุณาระบุ MC Number" });

                DateTime targetDate;
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                    targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                else
                    targetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

                string targetDDMM = $"{targetDate.Day:D2}{targetDate.Month:D2}";

                // ✅ REP MC — query แบบพิเศษ
                if (mcNo.ToUpper() == "REP")
                {
                    var repRecords = await _context.PoCheckFlows
                        .Where(p => p.McNo == "REP" &&
                               p.CheckDate.HasValue &&
                               p.CheckDate.Value.Year == targetDate.Year &&
                               p.CheckDate.Value.Month == targetDate.Month &&
                               p.CheckDate.Value.Day == targetDate.Day)
                        .ToListAsync();

                    var repData = repRecords.Select(r => new
                    {
                        id = r.Id,
                        poLot = r.PoLot,
                        imobileLot = (string?)null,
                        statusTn = r.StatusTn,
                        checkSt = r.CheckSt,
                        check = "OK",
                        checkDate = r.CheckDate,
                        mcNo = r.McNo,
                        lotQty = (int?)null,
                        rowColor = "default"
                    }).ToList<object>();

                    return Ok(new
                    {
                        success = true,
                        data = repData,
                        totalCount = repRecords.Count,
                        displayCount = repData.Count,
                        okCount = repRecords.Count,
                        ngCount = 0,
                        date = targetDate.ToString("yyyy-MM-dd")
                    });
                }

                var savedRecords = await _context.PoCheckFlows
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

                if (savedRecords.Count == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        data = new List<object>(),
                        totalCount = 0,
                        okCount = 0,
                        ngCount = 0,
                        message = $"ไม่พบข้อมูล LOT ของ MC {mcNo}"
                    });
                }

                // ✅ แก้: ParseNoPo normalize ตัวพิมพ์ก่อน split
                int ParseNoPo(string? poLot)
                {
                    var parts = poLot?.ToUpper().Split('-');
                    if (parts == null || parts.Length < 3) return 0;
                    var m = System.Text.RegularExpressions.Regex.Match(parts[2], @"^(\d+)");
                    return m.Success && int.TryParse(m.Groups[1].Value, out int n) ? n : 0;
                }

                int maxNoPo = savedRecords.Max(r => ParseNoPo(r.PoLot));

                var savedNoPoSet = savedRecords
                    .Select(r => ParseNoPo(r.PoLot))
                    .Where(n => n > 0)
                    .ToHashSet();

                var firstRecord = savedRecords.First();
                var firstParts = firstRecord.PoLot?.Split('-');
                if (firstParts == null || firstParts.Length < 2)
                    return Ok(new { success = true, data = new List<object>() });

                string lotPoPrefix = firstParts[0];

                var allThRecords = await _thicknessContext.ThRecords
                    .Where(t => t.LotPo == lotPoPrefix && t.McPo == mcNo)
                    .ToListAsync();

                // ✅ แก้: OrderBy TimeProcess ascending แล้ว overwrite → ได้ record ล่าสุดเสมอ
                var thByNoPo = new Dictionary<int, ThRecord>();
                foreach (var th in allThRecords.OrderBy(t => t.TimeProcess))
                {
                    var m = System.Text.RegularExpressions.Regex.Match(th.NoPo ?? "", @"^(\d+)");
                    if (m.Success && int.TryParse(m.Groups[1].Value, out int num))
                        thByNoPo[num] = th;
                }

                var combinedList = new List<object>();

                for (int i = 1; i <= maxNoPo; i++)
                {
                    if (savedNoPoSet.Contains(i))
                    {
                        var matched = savedRecords
                            .Where(p => ParseNoPo(p.PoLot) == i)
                            .OrderBy(p => p.PoLot?.ToUpper())
                            .ToList();

                        foreach (var s in matched)
                        {
                            combinedList.Add(new
                            {
                                id = s.Id,
                                poLot = s.PoLot,
                                imobileLot = s.Imobilelot,
                                statusTn = s.StatusTn,
                                checkSt = s.CheckSt,
                                check = s.CheckSt == true ? "OK" : "NG",
                                checkDate = s.CheckDate,
                                mcNo = s.McNo,
                                lotQty = s.LotQty,
                                rowColor = "default"
                            });
                        }
                    }
                    else if (thByNoPo.TryGetValue(i, out ThRecord? th))
                    {
                        string status = th.Status?.Trim() ?? "";
                        string statusUp = status.ToUpper();
                        string rowColor = "default";
                        string displaySt = status;
                        string checkVal = "SKIP";

                        if (statusUp == "SCRAP")
                        {
                            rowColor = "scrap";
                            checkVal = "SCRAP";
                        }
                        else if (statusUp == "HOLD")
                        {
                            rowColor = "hold";
                            checkVal = "HOLD";
                        }
                        else if (statusUp == "RESCREEN")
                        {
                            var rescreenRec = await _context.RescreenCheckRecords1
                                .FirstOrDefaultAsync(r => r.ImobileLot == th.ImobileLot);

                            if (rescreenRec != null)
                            {
                                rowColor = "waiting_rescreen";
                                displaySt = "Waiting Rescreen";
                            }
                            else
                            {
                                rowColor = "rescreen_pending";
                                displaySt = "Rescreen";
                            }
                            checkVal = "RESCREEN";
                        }

                        combinedList.Add(new
                        {
                            id = (Guid?)null,
                            poLot = $"{th.LotPo}-{th.McPo}-{th.NoPo}",
                            imobileLot = th.ImobileLot,
                            statusTn = displaySt,
                            checkSt = false,
                            check = checkVal,
                            checkDate = (DateTime?)null,
                            mcNo = mcNo,
                            lotQty = (int?)null,
                            rowColor = rowColor
                        });
                    }
                }

                int totalCount = savedRecords.Count;
                int okCount = savedRecords.Count(r => r.CheckSt == true);
                int ngCount = savedRecords.Count(r => r.CheckSt != true);

                return Ok(new
                {
                    success = true,
                    data = combinedList,
                    totalCount = totalCount,
                    displayCount = combinedList.Count,
                    okCount = okCount,
                    ngCount = ngCount,
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
                        (p.CheckDate.HasValue && p.CheckDate.Value == DateOnly.FromDateTime(targetDate))
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
                                                 (p.CheckDate.HasValue && p.CheckDate.Value == DateOnly.FromDateTime(targetDate))
                                                 ||
                                                 (p.PoLot != null && p.PoLot.Length >= 4 &&
                                                  p.PoLot.Substring(0, 4) == targetDDMM &&
                                                  p.PoLot.Contains($"-{mcNo}-"))
                                             ));
                }
                else
                {
                    query = query.Where(p =>
                        (p.CheckDate.HasValue && p.CheckDate.Value == DateOnly.FromDateTime(targetDate))
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
                        okCount = records.Count(r => r.CheckSt == true),
                        ngCount = records.Count(r => r.CheckSt != true && r.StatusTn?.ToUpper() != "SCRAP"),
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
                        check = r.StatusTn?.ToUpper() == "SCRAP" ? "SCRAP" : (r.CheckSt == true ? "OK" : "NG"),
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


        // ─────────────────────────────────────────────
        // Private Helper Methods
        // ─────────────────────────────────────────────

        private async Task<Th100Record?> FindTH100Record(ThRecord thRecord)
        {
            var noPoNumber = System.Text.RegularExpressions.Regex.Match(thRecord.NoPo ?? "", @"^(\d+)").Groups[1].Value;

            if (string.IsNullOrEmpty(noPoNumber))
                return null;

            var th100List = await _thicknessContext.Th100Records
                .Where(t => t.LotPo == thRecord.LotPo && t.McPo == thRecord.McPo)
                .ToListAsync();

            // ✅ แก้: OrdinalIgnoreCase + เลือก TimeProcess ล่าสุดเมื่อ NoPo ซ้ำ
            var matched = th100List
                .Where(t => {
                    var raw = System.Text.RegularExpressions.Regex.Match(t.NoPo ?? "", @"^(\d+)").Groups[1].Value;
                    return string.Equals(raw, noPoNumber, StringComparison.OrdinalIgnoreCase);
                })
                .OrderByDescending(t => t.TimeProcess)
                .FirstOrDefault();

            if (matched != null)
                return matched;

            // fallback: ค้นด้วย ImobileLot เลือกล่าสุด
            return await _thicknessContext.Th100Records
                .Where(t => t.ImobileLot == thRecord.ImobileLot)
                .OrderByDescending(t => t.TimeProcess)
                .FirstOrDefaultAsync();
        }

        private async Task AutoAddScrapLot(ThRecord thRecord)
        {
            if (thRecord == null) return;

            string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";

            var existingRecord = await _context.PoCheckFlows
                .FirstOrDefaultAsync(p => p.Imobilelot == thRecord.ImobileLot);

            if (existingRecord == null)
            {
                var checkDate = DateOnly.FromDateTime(DateTime.UtcNow);

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

        private async Task<(bool exists, bool isOK, string status)> CheckRescreenRecord(string imobileLot)
        {
            var record = await _context.RescreenCheckRecords1
                .FirstOrDefaultAsync(r => r.ImobileLot == imobileLot);

            if (record == null)
                return (false, false, "");

            return (true, true, record.FinalStatus ?? "");
        }

        private async Task<bool> CheckTH100Record(string lotPo, string mcPo, int noPoNumber)
        {
            var th100Records = await _thicknessContext.Th100Records
                .Where(t => t.LotPo == lotPo && t.McPo == mcPo)
                .ToListAsync();

            // ✅ แก้: เลือก TimeProcess ล่าสุดเมื่อ NoPo ซ้ำ
            var matched = th100Records
                .Where(t => {
                    var raw = System.Text.RegularExpressions.Regex.Match(t.NoPo ?? "", @"^(\d+)").Groups[1].Value;
                    return int.TryParse(raw, out int num) && num == noPoNumber;
                })
                .OrderByDescending(t => t.TimeProcess)
                .FirstOrDefault();

            return matched != null;
        }

        private async Task<(bool canSkip, bool canAdd, string source)> CheckRescreenLotStatus(
            string imobileLot, string lotPo, string mcPo, int noPoNumber)
        {
            var (exists, isOK, status) = await CheckRescreenRecord(imobileLot);
            if (exists && isOK)
                return (canSkip: true, canAdd: false, source: "rescreen_check_record");

            var hasTH100 = await CheckTH100Record(lotPo, mcPo, noPoNumber);
            if (hasTH100)
                return (canSkip: false, canAdd: true, source: "th100_record");

            return (canSkip: false, canAdd: false, source: "none");
        }

        private async Task<(bool isScrap, bool isHold, bool isRescreen, string status, ThRecord thRecord)> CheckLotStatusEnhanced(string lotPo, string mcPo, int noPoNumber)
        {
            var thRecords = await _thicknessContext.ThRecords
                .Where(t => t.LotPo == lotPo && t.McPo == mcPo)
                .ToListAsync();

            // ✅ แก้: กรอง NoPo ตรง แล้วเลือก TimeProcess ล่าสุด
            var matched = thRecords
                .Where(t => {
                    var raw = System.Text.RegularExpressions.Regex.Match(t.NoPo ?? "", @"^(\d+)").Groups[1].Value;
                    return int.TryParse(raw, out int num) && num == noPoNumber;
                })
                .OrderByDescending(t => t.TimeProcess)
                .FirstOrDefault();

            if (matched == null)
                return (false, false, false, "", null);

            if (!string.IsNullOrEmpty(matched.Status))
            {
                var statusLower = matched.Status.ToLower().Trim();
                if (statusLower == "scrap") return (true, false, false, matched.Status, matched);
                if (statusLower == "hold") return (false, true, false, matched.Status, matched);
                if (statusLower == "rescreen") return (false, false, true, matched.Status, matched);
            }

            return (false, false, false, matched.Status ?? "", matched);
        }

        // ─────────────────────────────────────────────
        // Request Models
        // ─────────────────────────────────────────────

        public class SearchLotRequest
        {
            public string? LotNumber { get; set; }
        }

        public class SaveLotRequest
        {
            public string? ImobileLot { get; set; }
            public int? LotQty { get; set; }
            public string? PoLot { get; set; }   // ✅ เพิ่มสำหรับ REP Product
        }
    }
}