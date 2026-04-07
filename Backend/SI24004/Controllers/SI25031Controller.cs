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

        // ✅ CT Suffix: ตรวจว่า NoPo ลงท้ายด้วย -C หรือ -T (case-insensitive)
        // เช่น "001-C", "002-T" → ต้องผ่านหน้า Rescreen ก่อนบันทึก check_flow
        private bool IsCTSuffix(string? noPo)
        {
            if (string.IsNullOrEmpty(noPo)) return false;
            var parts = noPo.Split('-');
            var lastPart = parts[^1].ToLower().Trim();
            return lastPart == "c" || lastPart == "t" || lastPart == "n";
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

                // ✅ PERF FIX (Bug1): Pre-fetch saved PoLots in 1 query แทน N+1 AnyAsync loop
                var searchCandidatePoLots = allThRecords
                    .Select(tr => $"{tr.LotPo}-{tr.McPo}-{tr.NoPo}")
                    .ToList();
                var searchSavedPoLotSet = (await _context.PoCheckFlows
                    .Where(p => searchCandidatePoLots.Contains(p.PoLot))
                    .Select(p => p.PoLot)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (isBrSize)
                {
                    // หา Colloidal ที่ยังไม่ถูกบันทึกก่อน (in-memory, no DB call)
                    thRecord = allThRecords
                        .Where(t => t.Process != null &&
                                    t.Process.Equals("Colloidal", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(t => t.TimeProcess)
                        .FirstOrDefault(tr => !searchSavedPoLotSet.Contains($"{tr.LotPo}-{tr.McPo}-{tr.NoPo}"));

                    // ถ้า Colloidal ถูกบันทึกหมดแล้ว หาตัวอื่นที่ยังไม่ถูกบันทึก (in-memory)
                    if (thRecord == null)
                    {
                        thRecord = allThRecords
                            .OrderByDescending(t => t.TimeProcess)
                            .FirstOrDefault(tr => !searchSavedPoLotSet.Contains($"{tr.LotPo}-{tr.McPo}-{tr.NoPo}"));
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
                    // ปกติ: หา ThRecord ที่ยังไม่มีใน po_check_flow (in-memory, no DB call)
                    thRecord = allThRecords
                        .OrderByDescending(t => t.TimeProcess)
                        .FirstOrDefault(tr => !searchSavedPoLotSet.Contains($"{tr.LotPo}-{tr.McPo}-{tr.NoPo}"));

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

                // ✅ PERF FIX: Pre-load data once, then run loop in-memory (no N+1 queries)
                if (currentNoPo > 1)
                {
                    var seqCtx = await PreloadForSequenceCheckAsync(thRecord.LotPo, thRecord.McPo, currentNoPo);
                    var existingNoPoSet = new HashSet<int>(existingNoPos);

                    var (missingNormalLots, addedScrap, skippedRescreen, canAddRescreen) =
                        await RunSequenceCheckAsync(thRecord.LotPo, thRecord.McPo, currentNoPo, existingNoPoSet, seqCtx);

                    autoAddedScrapLots  = addedScrap;
                    rescreenSkippedLots = skippedRescreen;
                    rescreenCanAddLots  = canAddRescreen;

                    if (missingNormalLots.Any())
                    {
                        var isFirstLot = !existingNoPos.Any();
                        var errorMessage = isFirstLot
                            ? $"❌ ไม่มี LOT ของ {thRecord.LotPo}-{thRecord.McPo} ในระบบ\n\n📋 กรุณาเริ่มต้นด้วย: {thRecord.LotPo}-{thRecord.McPo}-001\n\n🔍 LOT ที่พยายามเพิ่ม: {currentPoLot}{statusInfo}"
                            : $"❌ ไม่สามารถเพิ่ม LOT นี้ได้\n\n⚠️ ยังมี LOT ก่อนหน้าที่ยังไม่ได้เพิ่ม:\n• {string.Join("\n• ", missingNormalLots)}\n\n🔍 LOT ที่พยายามเพิ่ม: {currentPoLot}{statusInfo}";

                        if (rescreenSkippedLots.Any())
                            errorMessage += $"\n\n✅ LOT ที่ข้ามได้ (มีใน Rescreen Check):\n• {string.Join("\n• ", rescreenSkippedLots)}";
                        if (rescreenCanAddLots.Any())
                            errorMessage += $"\n\n📌 LOT Rescreen ที่ต้องเพิ่มก่อน (มีใน TH100):\n• {string.Join("\n• ", rescreenCanAddLots)}";

                        return BadRequest(new
                        {
                            success = false,
                            message = errorMessage,
                            requiredLot = isFirstLot ? $"{thRecord.LotPo}-{thRecord.McPo}-001" : (string?)null,
                            missingLots = missingNormalLots,
                            currentLot = currentPoLot,
                            status = thRecord.Status,
                            rescreenSkippedLots = rescreenSkippedLots,
                            rescreenCanAddLots  = rescreenCanAddLots
                        });
                    }

                    if (autoAddedScrapLots.Any())
                        statusInfo += $"\n\n✅ Auto-add SCRAP LOT:\n• {string.Join("\n• ", autoAddedScrapLots)}";
                    if (rescreenSkippedLots.Any())
                        statusInfo += $"\n\n✅ ข้าม Rescreen LOT (มีใน Rescreen Check):\n• {string.Join("\n• ", rescreenSkippedLots)}";
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

                // ✅ CT Suffix Rule: ถ้า NoPo ลงท้าย -C/-T (ทุก status ไม่ใช่แค่ OK)
                // → ต้องผ่านหน้า Rescreen Check ก่อนเสมอ ไม่ว่า TH_Record จะเป็น OK หรือ Rescreen
                bool requiresCTRescreen = false;
                if (IsCTSuffix(thRecord.NoPo))
                {
                    var ctRescreenRecord = await _context.RescreenCheckRecords1
                        .FirstOrDefaultAsync(r => r.ImobileLot == thRecord.ImobileLot);

                    if (ctRescreenRecord != null && ctRescreenRecord.FinalStatus?.ToUpper() == "OK")
                    {
                        // ผ่านหน้า Rescreen แล้วและ FinalStatus = OK → บันทึกได้
                        checkSt = true;
                        finalStatus = "OK (Rescreen)";
                        requiresCTRescreen = false;
                    }
                    else
                    {
                        // ยังไม่มีใน Rescreen Check หรือ FinalStatus ไม่ใช่ OK
                        // → บังคับไปลงหน้า Rescreen ก่อน ไม่ว่า status เดิมจะเป็นอะไร
                        checkSt = false;
                        finalStatus = "Rescreen";
                        requiresCTRescreen = true;
                    }
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
                        cassetteNo = thRecord.Ca5In9,
                        requiresCTRescreen = requiresCTRescreen
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

                // ✅ PERF FIX (Bug1): Pre-fetch saved PoLots in 1 query แทน N+1 AnyAsync loop
                var saveCandidatePoLots = allThRecordsSave
                    .Select(tr => $"{tr.LotPo}-{tr.McPo}-{tr.NoPo}")
                    .ToList();
                var saveSavedPoLotSet = (await _context.PoCheckFlows
                    .Where(p => saveCandidatePoLots.Contains(p.PoLot))
                    .Select(p => p.PoLot)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (isBrSizeSave)
                {
                    // หา Colloidal ที่ยังไม่ถูกบันทึกก่อน (in-memory, no DB call)
                    thRecord = allThRecordsSave
                        .Where(t => t.Process != null &&
                                    t.Process.Equals("Colloidal", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(t => t.TimeProcess)
                        .FirstOrDefault(tr => !saveSavedPoLotSet.Contains($"{tr.LotPo}-{tr.McPo}-{tr.NoPo}"));

                    // ถ้า Colloidal หมดแล้ว หาตัวอื่น (in-memory)
                    if (thRecord == null)
                    {
                        thRecord = allThRecordsSave
                            .OrderByDescending(t => t.TimeProcess)
                            .FirstOrDefault(tr => !saveSavedPoLotSet.Contains($"{tr.LotPo}-{tr.McPo}-{tr.NoPo}"));
                    }

                    if (thRecord == null)
                    {
                        return BadRequest(new { success = false, message = "ImobileLot นี้ถูกบันทึกครบทุก NoPo แล้ว", status = "DUPLICATE" });
                    }
                }
                else
                {
                    // ปกติ: หา ThRecord ที่ยังไม่มีใน po_check_flow (in-memory, no DB call)
                    thRecord = allThRecordsSave
                        .OrderByDescending(t => t.TimeProcess)
                        .FirstOrDefault(tr => !saveSavedPoLotSet.Contains($"{tr.LotPo}-{tr.McPo}-{tr.NoPo}"));

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

                // ✅ PERF FIX: Pre-load data once, then run loop in-memory (no N+1 queries)
                if (currentNoPo > 1)
                {
                    var seqCtx = await PreloadForSequenceCheckAsync(thRecord.LotPo, thRecord.McPo, currentNoPo);
                    var existingNoPoSet = new HashSet<int>(existingNoPos);

                    var (missingNormalLots, addedScrap, skippedRescreen, canAddRescreen) =
                        await RunSequenceCheckAsync(thRecord.LotPo, thRecord.McPo, currentNoPo, existingNoPoSet, seqCtx);

                    autoAddedScrapLots  = addedScrap;
                    rescreenSkippedLots = skippedRescreen;
                    rescreenCanAddLots  = canAddRescreen;

                    if (missingNormalLots.Any())
                    {
                        var isFirstLot = !existingNoPos.Any();
                        var validationMessage = isFirstLot
                            ? $"ไม่มี LOT ของ {thRecord.LotPo}-{thRecord.McPo} ในระบบ กรุณาเริ่มต้นด้วย {thRecord.LotPo}-{thRecord.McPo}-001"
                            : $"ไม่สามารถเพิ่ม LOT นี้ได้ เนื่องจากยังมี LOT ก่อนหน้าที่ยังไม่ได้เพิ่ม: {string.Join(", ", missingNormalLots)}";

                        if (rescreenSkippedLots.Any())
                            validationMessage += $"\n\n✅ LOT ที่ข้ามได้ (มีใน Rescreen Check): {string.Join(", ", rescreenSkippedLots)}";
                        if (rescreenCanAddLots.Any())
                            validationMessage += $"\n\n📌 LOT Rescreen ที่ต้องเพิ่มก่อน (มีใน TH100): {string.Join(", ", rescreenCanAddLots)}";

                        return BadRequest(new { success = false, message = validationMessage });
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

                    // ✅ CT Suffix Rule: NoPo ลงท้าย -C/-T (ทุก status ไม่ใช่แค่ OK)
                    // → ต้องผ่านหน้า Rescreen Check ก่อนเสมอ ไม่ว่า TH_Record จะเป็น OK หรือ Rescreen
                    if (IsCTSuffix(thRecord.NoPo))
                    {
                        var ctRescreenRecord = await _context.RescreenCheckRecords1
                            .FirstOrDefaultAsync(r => r.ImobileLot == thRecord.ImobileLot);

                        if (ctRescreenRecord == null || ctRescreenRecord.FinalStatus?.ToUpper() != "OK")
                        {
                            return BadRequest(new
                            {
                                success = false,
                                message = $"❌ LOT นี้เป็นประเภท -C/-T\n\n⚠️ ต้องลงในหน้า Rescreen ก่อนเท่านั้น\n\nกรุณาไปที่หน้า 'Rescreen Check' แล้วบันทึก LOT นี้ก่อน",
                                status = "NEEDS_RESCREEN",
                                requiresCTRescreen = true
                            });
                        }

                        // ผ่าน Rescreen แล้วและ FinalStatus = OK → บันทึกได้ด้วยสถานะ OK (Rescreen)
                        checkSt = true;
                        finalStatus = "OK (Rescreen)";
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
                    return BadRequest(new
                    {
                        success = false,
                        message = $"LOT {poLot} เคยถูกบันทึกแล้ว ไม่สามารถเพิ่มซ้ำได้",
                        status = "DUPLICATE"
                    });
                }

                // ถ้า ImobileLot ซ้ำแต่ PoLot ต่างกัน = พนักงานคีย์ผิด → อนุญาตให้บันทึกได้
                // แต่ต้อง set Imobilelot เป็น null เพื่อหลีกเลี่ยง unique constraint
                string? imobilelotToSave = thRecord.ImobileLot;
                if (!string.IsNullOrEmpty(thRecord.ImobileLot))
                {
                    var existingByImobileLot = await _context.PoCheckFlows
                        .AnyAsync(p => p.Imobilelot == thRecord.ImobileLot);
                    if (existingByImobileLot)
                        imobilelotToSave = null; // clear เพื่อหลีกเลี่ยง unique constraint
                }

                // PoLot ยังไม่มีใน DB → CREATE NEW
                poCheckFlow = new PoCheckFlow
                {
                    PoLot = poLot,
                    Imobilelot = imobilelotToSave,
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

                // ✅ PERF FIX: Pre-fetch rescreen records ก่อน loop — ไม่ต้อง query ทุก iteration
                var rescreenImobileLots = thByNoPo.Values
                    .Where(t => t.Status?.ToUpper() == "RESCREEN" && t.ImobileLot != null)
                    .Select(t => t.ImobileLot!)
                    .ToList();

                var rescreenInCheckSet = rescreenImobileLots.Any()
                    ? (await _context.RescreenCheckRecords1
                        .Where(r => rescreenImobileLots.Contains(r.ImobileLot))
                        .Select(r => r.ImobileLot)
                        .ToListAsync())
                        .ToHashSet(StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                            // ✅ PERF FIX: ใช้ HashSet ที่ pre-fetch ไว้แล้ว แทน query ในลูป
                            if (rescreenInCheckSet.Contains(th.ImobileLot ?? ""))
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
                return StatusCode(500, new { 
                    success = false, 
                    message = "เกิดข้อผิดพลาดในการดึงรายการ MC", 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    detail = ex.InnerException?.InnerException?.Message
                });
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

        // ─────────────────────────────────────────────────────────────────────────
        // ✅ PERF FIX: Pre-load helpers — eliminates N+1 queries in sequence check
        // ─────────────────────────────────────────────────────────────────────────

        private record SequenceCheckContext(
            Dictionary<int, ThRecord> ThByNoPo,
            HashSet<int> Th100NoPoSet,
            HashSet<string> RescreenApprovedImobileLots
        );

        /// <summary>
        /// Fetches all ThRecords, Th100Records, and RescreenCheck records for a
        /// LotPo+McPo combination in 3 bulk queries — to be used before sequence-check loops.
        /// </summary>
        private async Task<SequenceCheckContext> PreloadForSequenceCheckAsync(
            string lotPo, string mcPo, int currentNoPo)
        {
            // 1. All ThRecords for LotPo+McPo — build noPoNumber → latest record map
            var allTh = await _thicknessContext.ThRecords
                .Where(t => t.LotPo == lotPo && t.McPo == mcPo)
                .OrderBy(t => t.TimeProcess)
                .ToListAsync();

            var thByNoPo = new Dictionary<int, ThRecord>();
            foreach (var th in allTh)
            {
                var m = System.Text.RegularExpressions.Regex.Match(th.NoPo ?? "", @"^(\d+)");
                if (m.Success && int.TryParse(m.Groups[1].Value, out int n))
                    thByNoPo[n] = th; // overwrite → keeps latest TimeProcess
            }

            // 2. All Th100Records for LotPo+McPo — build noPoNumber set
            var allTh100 = await _thicknessContext.Th100Records
                .Where(t => t.LotPo == lotPo && t.McPo == mcPo)
                .ToListAsync();

            var th100NoPoSet = new HashSet<int>();
            foreach (var t in allTh100)
            {
                var m = System.Text.RegularExpressions.Regex.Match(t.NoPo ?? "", @"^(\d+)");
                if (m.Success && int.TryParse(m.Groups[1].Value, out int n))
                    th100NoPoSet.Add(n);
            }

            // 3. Collect imobileLots of rescreen lots in range → query RescreenCheck once
            var rescreenImobileLots = thByNoPo
                .Where(kv => kv.Key < currentNoPo &&
                             string.Equals(kv.Value.Status, "rescreen", StringComparison.OrdinalIgnoreCase) &&
                             kv.Value.ImobileLot != null)
                .Select(kv => kv.Value.ImobileLot!)
                .ToList();

            HashSet<string> rescreenApproved;
            if (rescreenImobileLots.Any())
            {
                // ✅ BUG FIX (Bug2): ใช้ FinalStatus=="OK" แทน IsApproved==true
                // เพื่อให้ตรงกับ logic ที่ UI แสดง "Waiting Rescreen" (สีส้ม)
                // → lot ที่บันทึกใน RescreenCheck และ FinalStatus=OK ข้ามได้ในลำดับ
                var approvedList = await _context.RescreenCheckRecords1
                    .Where(r => rescreenImobileLots.Contains(r.ImobileLot) &&
                                r.FinalStatus != null && r.FinalStatus.ToUpper() == "OK")
                    .Select(r => r.ImobileLot)
                    .ToListAsync();
                rescreenApproved = approvedList
                    .Where(x => x != null)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase)!;
            }
            else
            {
                rescreenApproved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return new SequenceCheckContext(thByNoPo, th100NoPoSet, rescreenApproved);
        }

        /// <summary>
        /// Runs the sequence-check loop entirely in-memory using pre-loaded data.
        /// Returns missing lots, auto-added scrap lots, and rescreen classification lists.
        /// SaveChanges for scrap lots is batched (single round-trip).
        /// </summary>
        private async Task<(
            List<string> missingNormalLots,
            List<string> autoAddedScrapLots,
            List<string> rescreenSkippedLots,
            List<string> rescreenCanAddLots
        )> RunSequenceCheckAsync(
            string lotPo, string mcPo, int currentNoPo,
            ISet<int> existingNoPos,
            SequenceCheckContext ctx)
        {
            var missingNormalLots   = new List<string>();
            var autoAddedScrapLots  = new List<string>();
            var rescreenSkippedLots = new List<string>();
            var rescreenCanAddLots  = new List<string>();
            var scrapLotsToAdd      = new List<ThRecord>();

            for (int i = 1; i < currentNoPo; i++)
            {
                if (existingNoPos.Contains(i)) continue;

                if (!ctx.ThByNoPo.TryGetValue(i, out var th))
                {
                    missingNormalLots.Add($"{lotPo}-{mcPo}-{i:D3}");
                    continue;
                }

                var statusLower = th.Status?.ToLower().Trim() ?? "";

                if (statusLower == "scrap")
                {
                    scrapLotsToAdd.Add(th);
                    autoAddedScrapLots.Add($"{lotPo}-{mcPo}-{th.NoPo}");
                }
                else if (statusLower == "rescreen")
                {
                    bool inRescreen = ctx.RescreenApprovedImobileLots.Contains(th.ImobileLot ?? "");
                    bool hasTh100   = ctx.Th100NoPoSet.Contains(i);

                    if (inRescreen)
                        rescreenSkippedLots.Add($"{lotPo}-{mcPo}-{i:D3}");
                    else if (hasTh100)
                    {
                        rescreenCanAddLots.Add($"{lotPo}-{mcPo}-{i:D3}");
                        missingNormalLots.Add($"{lotPo}-{mcPo}-{i:D3}");
                    }
                    else
                        missingNormalLots.Add($"{lotPo}-{mcPo}-{i:D3}");
                }
                else if (statusLower == "hold")
                {
                    // hold lots do not block progression — skip silently
                }
                else
                {
                    missingNormalLots.Add($"{lotPo}-{mcPo}-{i:D3}");
                }
            }

            // ✅ Batch: check existing scrap lots in one query, then single SaveChanges
            if (scrapLotsToAdd.Any())
            {
                var scrapImobileLots = scrapLotsToAdd
                    .Select(t => t.ImobileLot)
                    .Where(x => x != null)
                    .ToList();

                var alreadySaved = (await _context.PoCheckFlows
                    .Where(p => scrapImobileLots.Contains(p.Imobilelot))
                    .Select(p => p.Imobilelot)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var scrapTh in scrapLotsToAdd)
                {
                    if (alreadySaved.Contains(scrapTh.ImobileLot ?? "")) continue;
                    _context.PoCheckFlows.Add(new PoCheckFlow
                    {
                        PoLot      = $"{scrapTh.LotPo}-{scrapTh.McPo}-{scrapTh.NoPo}",
                        Imobilelot = scrapTh.ImobileLot,
                        StatusTn   = "SCRAP",
                        CheckSt    = false,
                        CheckDate  = DateOnly.FromDateTime(DateTime.UtcNow),
                        McNo       = scrapTh.McPo,
                        LotQty     = 0
                    });
                }
                await _context.SaveChangesAsync();
            }

            return (missingNormalLots, autoAddedScrapLots, rescreenSkippedLots, rescreenCanAddLots);
        }

        // ─────────────────────────────────────────────────────────────────────────

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