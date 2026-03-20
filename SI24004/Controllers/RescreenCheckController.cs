using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SI24004.Models;
using SI24004.ModelsSQL;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RescreenCheckController : ControllerBase
    {
        private readonly PostgrestContext _context;
        private readonly ThicknessContext _thicknessContext;

        public RescreenCheckController(PostgrestContext context, ThicknessContext thicknessContext)
        {
            _context = context;
            _thicknessContext = thicknessContext;
        }

        // Helper: คืนเวลา UTC+7
        private DateTime GetBangkokNow()
        {
            try
            {
                var bangkokZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, bangkokZone);
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    var bangkokZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, bangkokZone);
                }
                catch
                {
                    return DateTime.UtcNow.AddHours(7);
                }
            }
        }

        // Helper: filter ช่วงเวลา Bangkok time
        private (DateTime start, DateTime end) GetBangkokDayRange(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);
            return (startOfDay, endOfDay);
        }

        // ✅ แก้: FindTH100Record — OrdinalIgnoreCase + เลือก TimeProcess ล่าสุด
        private async Task<Th100Record?> FindTH100Record(ThRecord thRecord)
        {
            var noPoNumber = System.Text.RegularExpressions.Regex.Match(
                thRecord.NoPo ?? "", @"^(\d+)").Groups[1].Value;

            if (string.IsNullOrEmpty(noPoNumber))
                return null;

            var th100List = await _thicknessContext.Th100Records
                .Where(t => t.LotPo == thRecord.LotPo && t.McPo == thRecord.McPo)
                .ToListAsync();

            return th100List
                .Where(t => {
                    var raw = System.Text.RegularExpressions.Regex.Match(t.NoPo ?? "", @"^(\d+)").Groups[1].Value;
                    return string.Equals(raw, noPoNumber, StringComparison.OrdinalIgnoreCase);
                })
                .OrderByDescending(t => t.TimeProcess)
                .FirstOrDefault();
        }

        private async Task<PoCheckFlow?> FindPoCheckFlowRecord(string imobileLot)
        {
            return await _context.PoCheckFlows
                .FirstOrDefaultAsync(p => p.Imobilelot == imobileLot);
        }

        private (string finalStatus, bool isApproved, string approvedSource, string? th100Status) EvaluateStatus(
            Th100Record? th100Record, PoCheckFlow? poCheckFlow)
        {
            string? th100Status = th100Record?.Status;

            if (poCheckFlow != null)
                return ("OK", true, "Approved", th100Status);

            if (th100Record != null)
            {
                var upper = th100Record.Status?.ToUpper() ?? "";
                if (upper == "OK")
                    return ("OK", true, "TH100 Confirm", th100Status);

                return (th100Record.Status ?? "Rescreen", false, "TH100 Confirm", th100Status);
            }

            return ("Rescreen", false, "Pending", null);
        }

        [HttpPost("search-rescreen-lot")]
        public async Task<IActionResult> SearchRescreenLot([FromBody] SearchRescreenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ImobileLot))
                    return BadRequest(new { success = false, message = "กรุณา Scan Barcode" });

                var thRecord = await _thicknessContext.ThRecords
                    .Where(t => t.ImobileLot == request.ImobileLot)
                    .OrderByDescending(t => t.TimeProcess)
                    .FirstOrDefaultAsync();

                if (thRecord == null)
                    return NotFound(new { success = false, message = $"ไม่พบ LOT: {request.ImobileLot}" });

                if (string.IsNullOrEmpty(thRecord.Status) || thRecord.Status.ToLower() != "rescreen")
                    return BadRequest(new
                    {
                        success = false,
                        message = $"LOT นี้ไม่ใช่ Rescreen\n\nStatus ปัจจุบัน: {thRecord.Status ?? "N/A"}"
                    });

                var existingRecord = await _context.RescreenCheckRecords1
                    .FirstOrDefaultAsync(r => r.ImobileLot == request.ImobileLot);

                if (existingRecord != null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "LOT นี้ถูกเพิ่มแล้ว",
                        isDuplicate = true,
                        data = new
                        {
                            id = existingRecord.Id,
                            imobileLot = existingRecord.ImobileLot,
                            poLot = $"{existingRecord.LotPo}-{existingRecord.McPo}-{existingRecord.NoPo}",
                            mc = existingRecord.McPo,
                            status = existingRecord.Status,
                            th100Status = existingRecord.Th100Status,
                            finalStatus = existingRecord.FinalStatus,
                            isApproved = existingRecord.IsApproved,
                            approvedSource = existingRecord.ApprovedSource,
                            checkDate = existingRecord.CheckDate,
                            checkedBy = existingRecord.CheckedBy,
                            remarks = existingRecord.Remarks
                        }
                    });
                }

                var th100Record = await FindTH100Record(thRecord);
                var poCheckFlow = await FindPoCheckFlowRecord(thRecord.ImobileLot);
                var (finalStatus, isApproved, approvedSource, th100Status) =
                    EvaluateStatus(th100Record, poCheckFlow);

                return Ok(new
                {
                    success = true,
                    message = "พบข้อมูล Rescreen LOT",
                    isDuplicate = false,
                    data = new
                    {
                        imobileLot = thRecord.ImobileLot,
                        poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}",
                        mc = thRecord.McPo,
                        lotPo = thRecord.LotPo,
                        mcPo = thRecord.McPo,
                        noPo = thRecord.NoPo,
                        status = thRecord.Status,
                        th100Status = th100Status,
                        finalStatus = finalStatus,
                        isApproved = isApproved,
                        approvedSource = approvedSource,
                        timeProcess = thRecord.TimeProcess,
                        hasTH100 = th100Record != null,
                        hasPoCheckFlow = poCheckFlow != null
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }

        [HttpPost("quick-add-rescreen")]
        public async Task<IActionResult> QuickAddRescreen([FromBody] QuickAddRescreenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ImobileLot))
                    return BadRequest(new { success = false, message = "กรุณา Scan Imobile LOT" });

                var thRecord = await _thicknessContext.ThRecords
                    .Where(t => t.ImobileLot == request.ImobileLot)
                    .OrderByDescending(t => t.TimeProcess)
                    .FirstOrDefaultAsync();

                if (thRecord == null)
                    return NotFound(new { success = false, message = $"ไม่พบ LOT: {request.ImobileLot}" });

                if (string.IsNullOrEmpty(thRecord.Status) || thRecord.Status.ToLower() != "rescreen")
                    return BadRequest(new
                    {
                        success = false,
                        message = $"LOT นี้ไม่ใช่ Rescreen\n\nStatus ปัจจุบัน: {thRecord.Status ?? "N/A"}"
                    });

                var existingRecord = await _context.RescreenCheckRecords1
                    .FirstOrDefaultAsync(r => r.ImobileLot == request.ImobileLot);

                if (existingRecord != null)
                    return BadRequest(new
                    {
                        success = false,
                        message = $"LOT นี้ถูกเพิ่มแล้ว\n\nPO LOT: {existingRecord.LotPo}-{existingRecord.McPo}-{existingRecord.NoPo}\nStatus: {existingRecord.FinalStatus}"
                    });

                var th100Record = await FindTH100Record(thRecord);
                var poCheckFlow = await FindPoCheckFlowRecord(thRecord.ImobileLot);
                var (finalStatus, isApproved, approvedSource, th100Status) =
                    EvaluateStatus(th100Record, poCheckFlow);

                var checkDate = GetBangkokNow();

                var newRecord = new RescreenCheckRecord1
                {
                    Id = Guid.NewGuid(),
                    ImobileLot = thRecord.ImobileLot,
                    LotPo = thRecord.LotPo,
                    McPo = thRecord.McPo,
                    NoPo = thRecord.NoPo,
                    Status = thRecord.Status,
                    DateProcess = thRecord.DateProcess,
                    CheckDate = checkDate,
                    CheckedBy = request.CheckedBy,
                    Th100Status = th100Status,
                    FinalStatus = finalStatus,
                    IsApproved = isApproved,
                    ApprovedSource = approvedSource,
                    Remarks = "Quick add - Auto status from TH100/PO Check Flow"
                };

                _context.RescreenCheckRecords1.Add(newRecord);
                await _context.SaveChangesAsync();

                string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";

                return Ok(new
                {
                    success = true,
                    message = $"เพิ่ม Rescreen LOT สำเร็จ\n\nPO LOT: {poLot}\nStatus: {finalStatus}\nApproved By: {approvedSource}",
                    data = new
                    {
                        id = newRecord.Id,
                        imobileLot = newRecord.ImobileLot,
                        poLot = poLot,
                        mc = newRecord.McPo,
                        finalStatus = finalStatus,
                        isApproved = isApproved,
                        approvedSource = approvedSource,
                        th100Status = th100Status,
                        checkDate = checkDate
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }

        [HttpPost("save-rescreen-check")]
        public async Task<IActionResult> SaveRescreenCheck([FromBody] SaveRescreenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ImobileLot))
                    return BadRequest(new { success = false, message = "กรุณาระบุ ImobileLot" });

                var thRecord = await _thicknessContext.ThRecords
                    .Where(t => t.ImobileLot == request.ImobileLot)
                    .OrderByDescending(t => t.TimeProcess)
                    .FirstOrDefaultAsync();

                if (thRecord == null)
                    return NotFound(new { success = false, message = "ไม่พบ LOT" });

                if (string.IsNullOrEmpty(thRecord.Status) || thRecord.Status.ToLower() != "rescreen")
                    return BadRequest(new { success = false, message = "LOT นี้ไม่ใช่ Rescreen" });

                var th100Record = await FindTH100Record(thRecord);
                var poCheckFlow = await FindPoCheckFlowRecord(thRecord.ImobileLot);
                var (autoFinalStatus, autoIsApproved, autoApprovedSource, th100Status) =
                    EvaluateStatus(th100Record, poCheckFlow);

                string finalStatus = request.FinalStatus ?? autoFinalStatus;
                bool isApproved = request.IsApproved ?? autoIsApproved;
                string approvedSource = autoApprovedSource;

                var existingRecord = await _context.RescreenCheckRecords1
                    .FirstOrDefaultAsync(r => r.ImobileLot == request.ImobileLot);

                var checkDate = GetBangkokNow();

                if (existingRecord != null)
                {
                    existingRecord.Th100Status = th100Status;
                    existingRecord.FinalStatus = finalStatus;
                    existingRecord.IsApproved = isApproved;
                    existingRecord.ApprovedSource = approvedSource;
                    existingRecord.CheckDate = checkDate;
                    existingRecord.CheckedBy = request.CheckedBy;
                    existingRecord.Remarks = request.Remarks;
                }
                else
                {
                    var newRecord = new RescreenCheckRecord1
                    {
                        Id = Guid.NewGuid(),
                        ImobileLot = thRecord.ImobileLot,
                        LotPo = thRecord.LotPo,
                        McPo = thRecord.McPo,
                        NoPo = thRecord.NoPo,
                        Status = thRecord.Status,
                        DateProcess = thRecord.DateProcess,
                        CheckDate = checkDate,
                        CheckedBy = request.CheckedBy,
                        Th100Status = th100Status,
                        FinalStatus = finalStatus,
                        IsApproved = isApproved,
                        ApprovedSource = approvedSource,
                        Remarks = request.Remarks
                    };
                    _context.RescreenCheckRecords1.Add(newRecord);
                }

                await _context.SaveChangesAsync();
                string poLot = $"{thRecord.LotPo}-{thRecord.McPo}-{thRecord.NoPo}";

                return Ok(new
                {
                    success = true,
                    message = $"บันทึก Rescreen LOT {poLot} สำเร็จ",
                    data = new
                    {
                        imobileLot = thRecord.ImobileLot,
                        poLot = poLot,
                        mc = thRecord.McPo,
                        finalStatus = finalStatus,
                        isApproved = isApproved,
                        approvedSource = approvedSource,
                        checkDate = checkDate
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }

        // ✅ เพิ่ม mc = r.McPo ใน response เพื่อให้ frontend ใช้ Select MC filter ได้
        [HttpGet("get-rescreen-records")]
        public async Task<IActionResult> GetRescreenRecords(
            [FromQuery] string? mcNo,
            [FromQuery] string? date,
            [FromQuery] string? status)
        {
            try
            {
                DateTime? targetDate = null;
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                    targetDate = parsedDate.Date;

                var query = _context.RescreenCheckRecords1.AsQueryable();

                if (!string.IsNullOrEmpty(mcNo))
                    query = query.Where(r => r.McPo == mcNo);

                if (targetDate.HasValue)
                {
                    var (startOfDay, endOfDay) = GetBangkokDayRange(targetDate.Value);
                    query = query.Where(r => r.DateProcess >= startOfDay &&
                                            r.DateProcess < endOfDay);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    if (status.ToLower() == "approved")
                        query = query.Where(r => r.IsApproved == true);
                    else if (status.ToLower() == "pending")
                        query = query.Where(r => r.IsApproved == false);
                    else
                        query = query.Where(r => r.FinalStatus != null &&
                                                 r.FinalStatus.ToLower() == status.ToLower());
                }

                var records = await query.ToListAsync();

                records = records
                    .OrderBy(r => r.LotPo)
                    .ThenBy(r => {
                        var parts = (r.NoPo ?? "").ToUpper().Split('-');
                        return int.TryParse(parts[0], out int n) ? n : 0;
                    })
                    .ThenBy(r => {
                        var parts = (r.NoPo ?? "").ToUpper().Split('-');
                        return parts.Length > 1 ? parts[1] : "";
                    })
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = records.Select(r => new
                    {
                        id = r.Id,
                        imobileLot = r.ImobileLot,
                        poLot = $"{r.LotPo}-{r.McPo}-{r.NoPo}",
                        mc = r.McPo,   // ✅ เพิ่ม field นี้เพื่อให้ frontend groupBy / filter MC ได้
                        status = r.Status,
                        th100Status = r.Th100Status,
                        finalStatus = r.FinalStatus,
                        isApproved = r.IsApproved,
                        approvedSource = r.ApprovedSource,
                        checkDate = r.CheckDate,
                        dateProcess = r.DateProcess,
                        checkedBy = r.CheckedBy,
                        remarks = r.Remarks
                    }).ToList(),
                    summary = new
                    {
                        totalCount = records.Count,
                        approvedCount = records.Count(r => r.IsApproved),
                        pendingCount = records.Count(r => !r.IsApproved),
                        okCount = records.Count(r => r.FinalStatus?.ToLower() == "ok"),
                        holdCount = records.Count(r => r.FinalStatus?.ToLower() == "hold"),
                        date = targetDate?.ToString("yyyy-MM-dd")
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }

        [HttpPut("update-rescreen-status/{id}")]
        public async Task<IActionResult> UpdateRescreenStatus(Guid id, [FromBody] UpdateRescreenRequest request)
        {
            try
            {
                var record = await _context.RescreenCheckRecords1.FindAsync(id);
                if (record == null)
                    return NotFound(new { success = false, message = "ไม่พบ Rescreen LOT" });

                var oldFinalStatus = record.FinalStatus;
                var oldIsApproved = record.IsApproved;

                if (!string.IsNullOrEmpty(request.FinalStatus))
                    record.FinalStatus = request.FinalStatus;
                if (request.IsApproved.HasValue)
                    record.IsApproved = request.IsApproved.Value;
                if (!string.IsNullOrEmpty(request.Remarks))
                    record.Remarks = request.Remarks;

                record.CheckDate = GetBangkokNow();
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"อัปเดต Rescreen LOT สำเร็จ\n\nPO LOT: {record.LotPo}-{record.McPo}-{record.NoPo}",
                    data = new
                    {
                        id = record.Id,
                        imobileLot = record.ImobileLot,
                        poLot = $"{record.LotPo}-{record.McPo}-{record.NoPo}",
                        mc = record.McPo,
                        oldFinalStatus,
                        newFinalStatus = record.FinalStatus,
                        oldIsApproved,
                        newIsApproved = record.IsApproved,
                        checkDate = record.CheckDate
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }

        [HttpDelete("delete-rescreen-record/{id}")]
        public async Task<IActionResult> DeleteRescreenRecord(Guid id)
        {
            try
            {
                var record = await _context.RescreenCheckRecords1.FindAsync(id);
                if (record == null)
                    return NotFound(new { success = false, message = "ไม่พบข้อมูล" });

                _context.RescreenCheckRecords1.Remove(record);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"ลบ Rescreen LOT สำเร็จ\n\nPO LOT: {record.LotPo}-{record.McPo}-{record.NoPo}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }

        [HttpGet("check-lot-in-rescreen")]
        public async Task<IActionResult> CheckLotInRescreen([FromQuery] string imobileLot)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imobileLot))
                    return BadRequest(new { success = false, message = "กรุณาระบุ Imobile LOT" });

                var record = await _context.RescreenCheckRecords1
                    .FirstOrDefaultAsync(r => r.ImobileLot == imobileLot);

                if (record == null)
                    return Ok(new { success = true, exists = false, message = "LOT ยังไม่อยู่ใน Rescreen Check" });

                return Ok(new
                {
                    success = true,
                    exists = true,
                    data = new
                    {
                        imobileLot = record.ImobileLot,
                        poLot = $"{record.LotPo}-{record.McPo}-{record.NoPo}",
                        mc = record.McPo,
                        finalStatus = record.FinalStatus,
                        isApproved = record.IsApproved,
                        approvedSource = record.ApprovedSource,
                        checkDate = record.CheckDate
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }

        [HttpGet("get-summary-stats")]
        public async Task<IActionResult> GetSummaryStats([FromQuery] string? date)
        {
            try
            {
                DateTime targetDate = !string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsed)
                    ? parsed.Date
                    : GetBangkokNow().Date;

                var (startOfDay, endOfDay) = GetBangkokDayRange(targetDate);

                var records = await _context.RescreenCheckRecords1
                    .Where(r => r.DateProcess >= startOfDay &&
                                r.DateProcess < endOfDay)
                    .ToListAsync();

                var mcGroups = records.GroupBy(r => r.McPo)
                    .Select(g => new
                    {
                        mcNo = g.Key,
                        totalCount = g.Count(),
                        approvedCount = g.Count(r => r.IsApproved),
                        okCount = g.Count(r => r.FinalStatus?.ToLower() == "ok"),
                        holdCount = g.Count(r => r.FinalStatus?.ToLower() == "hold")
                    })
                    .OrderBy(g => g.mcNo)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    date = targetDate.ToString("yyyy-MM-dd"),
                    summary = new
                    {
                        totalCount = records.Count,
                        approvedCount = records.Count(r => r.IsApproved),
                        pendingCount = records.Count(r => !r.IsApproved),
                        okCount = records.Count(r => r.FinalStatus?.ToLower() == "ok"),
                        holdCount = records.Count(r => r.FinalStatus?.ToLower() == "hold"),
                        rescreenCount = records.Count(r => r.FinalStatus?.ToLower() == "rescreen")
                    },
                    byMachine = mcGroups
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }

        [HttpPost("refresh-rescreen-status")]
        public async Task<IActionResult> RefreshRescreenStatus([FromQuery] string? date)
        {
            try
            {
                IQueryable<RescreenCheckRecord1> query = _context.RescreenCheckRecords1
                    .Where(r => r.ApprovedSource != "Approved");

                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    var (startOfDay, endOfDay) = GetBangkokDayRange(parsedDate.Date);
                    query = query.Where(r => r.DateProcess >= startOfDay &&
                                            r.DateProcess < endOfDay);
                }

                var pendingRecords = await query.ToListAsync();
                var updatedList = new List<object>();

                foreach (var record in pendingRecords)
                {
                    var thRecord = await _thicknessContext.ThRecords
                        .Where(t => t.ImobileLot == record.ImobileLot)
                        .OrderByDescending(t => t.TimeProcess)
                        .FirstOrDefaultAsync();

                    if (thRecord == null) continue;

                    var th100Record = await FindTH100Record(thRecord);
                    var poCheckFlow = await FindPoCheckFlowRecord(record.ImobileLot);
                    var (finalStatus, isApproved, approvedSource, th100Status) =
                        EvaluateStatus(th100Record, poCheckFlow);

                    bool changed = false;

                    if (record.Th100Status != th100Status)
                    {
                        record.Th100Status = th100Status;
                        changed = true;
                    }

                    if (isApproved)
                    {
                        record.FinalStatus = finalStatus;
                        record.IsApproved = true;
                        record.ApprovedSource = approvedSource;
                        record.CheckDate = GetBangkokNow();
                        changed = true;
                    }

                    if (changed)
                    {
                        updatedList.Add(new
                        {
                            imobileLot = record.ImobileLot,
                            poLot = $"{record.LotPo}-{record.McPo}-{record.NoPo}",
                            mc = record.McPo,
                            th100Status = th100Status,
                            finalStatus = record.FinalStatus,
                            isApproved = record.IsApproved,
                            approvedSource = record.ApprovedSource
                        });
                    }
                }

                if (updatedList.Any())
                    await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = updatedList.Any()
                        ? $"อัปเดต {updatedList.Count} LOT สำเร็จ"
                        : "ไม่มี LOT ที่ต้องอัปเดต",
                    updatedCount = updatedList.Count,
                    updatedLots = updatedList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }

        // ---- Request DTOs ----
        public class SearchRescreenRequest { public string? ImobileLot { get; set; } }

        public class SaveRescreenRequest
        {
            public string? ImobileLot { get; set; }
            public string? FinalStatus { get; set; }
            public bool? IsApproved { get; set; }
            public string? CheckedBy { get; set; }
            public string? Remarks { get; set; }
        }

        public class QuickAddRescreenRequest
        {
            public string? ImobileLot { get; set; }
            public string? CheckedBy { get; set; }
        }

        public class UpdateRescreenRequest
        {
            public string? FinalStatus { get; set; }
            public bool? IsApproved { get; set; }
            public string? Remarks { get; set; }
        }
    }
}