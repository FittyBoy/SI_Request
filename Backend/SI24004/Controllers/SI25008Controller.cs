using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SI24004.Models.PostgreSQL;
using SI24004.Services;
using SI24004.Models.DTOs;
using System.Linq;
using SI24004.Models.DTOs;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using SI24004.Models.SqlServer;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SI25008Controller : ControllerBase
    {
        private readonly PostgrestContext _context;
        private readonly ThicknessContext _sqlcontext;
        private readonly SmtpSettings _smtpSettings;
        private readonly Models.DTOs.EmailRecipients _emailRecipients; // ✅ กลับไปใช้ EmailRecipients

        // กำหนด Thailand Time Zone
        private readonly TimeZoneInfo _thailandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        // ✅ กลับไปใช้ constructor แบบเดิม
        public SI25008Controller(
            PostgrestContext context,
            ThicknessContext sqlcontext,
            IOptions<SmtpSettings> smtpSettings,
            IOptions<Models.DTOs.EmailRecipients> emailRecipients) // ✅ กลับไปใช้ IOptions<EmailRecipients>
        {
            _context = context;
            _sqlcontext = sqlcontext;
            _smtpSettings = smtpSettings.Value;
            _emailRecipients = emailRecipients.Value; // ✅ กลับไปใช้ EmailRecipients
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ThRecordDTO>>> GetPolishingDataThickness(
            [FromQuery] string? date = null)
        {
            try
            {
                var currentThailandTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _thailandTimeZone);

                DateTime filterStart, filterEnd;

                if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var selectedDate))
                {
                    // ดึงข้อมูลเฉพาะวันที่ select (00:00:00 - 23:59:59)
                    filterStart = selectedDate.Date;
                    filterEnd   = selectedDate.Date.AddDays(1).AddTicks(-1);
                    Console.WriteLine($"📅 Filtering by selected date: {filterStart:yyyy-MM-dd}");
                }
                else
                {
                    // fallback: ย้อนหลัง 14 วัน
                    filterStart = currentThailandTime.AddDays(-14);
                    filterEnd   = currentThailandTime;
                    Console.WriteLine($"📅 Filtering last 14 days from {filterStart:yyyy-MM-dd}");
                }

                List<ThRecord> thRecords = await _sqlcontext.ThRecords
                    .Where(th => th.DateProcess >= filterStart
                              && th.DateProcess <= filterEnd
                              && th.ImobileType == "Polishing")
                    .OrderByDescending(th => th.DateProcess)
                    .ToListAsync();

                Console.WriteLine($"🔍 Found {thRecords.Count} records");

                // ใช้ LINQ Select แทนการ loop แบบเก่า
                var thRecordDTOs = thRecords.Select((th, index) =>
                {
                    try
                    {
                        var dto = new ThRecordDTO
                        {
                            // Basic fields
                            LotId = th.LotId,
                            LotPo = SafeString(th.LotPo),
                            McPo = SafeString(th.McPo),
                            NoPo = SafeString(th.NoPo),
                            MemberId = SafeString(th.MemberId),
                            Status = SafeString(th.Status),
                            Result = SafeString(th.Result),
                            Remark = SafeString(th.Remark),
                            ImobileLot = SafeString(th.ImobileLot),
                            ImobileType = SafeString(th.ImobileType),
                            ImobileSize = SafeString(th.ImobileSize),
                            McType = SafeString(th.McType),
                            Process = SafeString(th.Process),
                            Hostname = SafeString(th.Hostname),
                            IpAddress = SafeString(th.Ipaddress),
                            LaserMC = SafeString(th.LaserMc),
                            ProcessStep = th.ProcessStep,

                            // DateTime fields - แปลงเป็นเวลาไทย
                            DateProcess = ConvertToThailandTime(th.DateProcess),
                            TimeProcess = ConvertToThailandTime(th.TimeProcess),

                            // String fields that represent decimals
                            ThBefore = SafeString(th.ThBefore),
                            AvgTh = SafeString(th.AvgTh),
                            ProcessTime = SafeString(th.ProcessTime),
                            PoRate = SafeString(th.PoRate),
                            ThDif = SafeString(th.ThDif),
                            Margin = SafeString(th.Margin),
                            ThCin = SafeString(th.ThCin),
                            ThCout1 = SafeString(th.ThCout1),
                            ThCout2 = SafeString(th.ThCout2),
                            ThCout3 = SafeString(th.ThCout3),
                            ThCout4 = SafeString(th.ThCout4),
                            ThCout5 = SafeString(th.ThCout5),

                            // Ca arrays - main (1-5)
                            Ca1In = SafeConvertToDecimalList(th.Ca1In1, th.Ca1In2, th.Ca1In3, th.Ca1In4, th.Ca1In5),
                            Ca1Out = SafeConvertToDecimalList(th.Ca1Out1, th.Ca1Out2, th.Ca1Out3, th.Ca1Out4, th.Ca1Out5),
                            Ca2In = SafeConvertToDecimalList(th.Ca2In1, th.Ca2In2, th.Ca2In3, th.Ca2In4, th.Ca2In5),
                            Ca2Out = SafeConvertToDecimalList(th.Ca2Out1, th.Ca2Out2, th.Ca2Out3, th.Ca2Out4, th.Ca2Out5),
                            Ca3In = SafeConvertToDecimalList(th.Ca3In1, th.Ca3In2, th.Ca3In3, th.Ca3In4, th.Ca3In5),
                            Ca3Out = SafeConvertToDecimalList(th.Ca3Out1, th.Ca3Out2, th.Ca3Out3, th.Ca3Out4, th.Ca3Out5),
                            Ca4In = SafeConvertToDecimalList(th.Ca4In1, th.Ca4In2, th.Ca4In3, th.Ca4In4, th.Ca4In5),
                            Ca4Out = SafeConvertToDecimalList(th.Ca4Out1, th.Ca4Out2, th.Ca4Out3, th.Ca4Out4, th.Ca4Out5),
                            Ca5In = SafeConvertToDecimalList(th.Ca5In1, th.Ca5In2, th.Ca5In3, th.Ca5In4, th.Ca5In5),
                            Ca5Out = SafeConvertToDecimalList(th.Ca5Out1, th.Ca5Out2, th.Ca5Out3, th.Ca5Out4, th.Ca5Out5),

                            // Ca arrays - extra (6-10)
                            Ca1InExtra = SafeConvertToDecimalList(th.Ca1In6, th.Ca1In7, th.Ca1In8, th.Ca1In9, th.Ca1In10),
                            Ca1OutExtra = SafeConvertToDecimalList(th.Ca1Out6, th.Ca1Out7, th.Ca1Out8, th.Ca1Out9, th.Ca1Out10),
                            Ca2InExtra = SafeConvertToDecimalList(th.Ca2In6, th.Ca2In7, th.Ca2In8, th.Ca2In9, th.Ca2In10),
                            Ca3InExtra = SafeConvertToDecimalList(th.Ca3In6, th.Ca3In7, th.Ca3In8, th.Ca3In9, th.Ca3In10),
                            Ca4InExtra = SafeConvertToDecimalList(th.Ca4In6, th.Ca4In7, th.Ca4In8, th.Ca4In9, th.Ca4In10),
                            Ca5InExtra = SafeConvertToDecimalList(th.Ca5In6, th.Ca5In7, th.Ca5In8, th.Ca5In9, th.Ca5In10),
                            ThCinExtra = SafeConvertToDecimalList(th.ThCin2, th.ThCin3, th.ThCin4, th.ThCin5)
                        };

                        return dto;
                    }
                    catch (Exception recordEx)
                    {
                        Console.WriteLine($"🔥 Error processing record {index + 1}: {recordEx.Message}");
                        Console.WriteLine($"🔥 Record details: LotId={th.LotId}, Error: {recordEx}");
                        return null; // Return null for failed records
                    }
                })
                .Where(dto => dto != null) // Filter out null records
                .ToList();

                return Ok(thRecordDTOs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 ERROR in GetPolishingDataThickness(): " + ex.ToString());
                return StatusCode(500, $"🔥 Server Error: {ex.Message}");
            }
        }

        // เพิ่ม method สำหรับแปลงเวลาเป็นเวลาไทย
        private DateTime ConvertToThailandTime(DateTime dateTime)
        {
            // ถ้าเป็น Unspecified ให้ถือว่าเป็น UTC
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }

            // ถ้าเป็น Local Time ให้แปลงเป็น UTC ก่อน
            if (dateTime.Kind == DateTimeKind.Local)
            {
                dateTime = dateTime.ToUniversalTime();
            }

            // แปลงจาก UTC เป็นเวลาไทย
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, _thailandTimeZone);
        }

        // หรือหากต้องการแปลง DateTime? (nullable)
        private DateTime? ConvertToThailandTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return null;

            return ConvertToThailandTime(dateTime.Value);
        }

        // ปรับปรุง method สำหรับจัดการ string อย่างปลอดภัย
        private string SafeString(string input)
        {
            return string.IsNullOrWhiteSpace(input) ? null : input.Trim();
        }

        // ปรับปรุง method สำหรับแปลง decimal list โดยรับ parameters แบบ variadic
        private List<decimal?> SafeConvertToDecimalList(params string[] values)
        {
            var result = new List<decimal?>();

            foreach (var value in values)
            {
                result.Add(SafeConvertToDecimal(value));
            }

            return result;
        }

        // ปรับปรุง method สำหรับแปลง decimal ให้ปลอดภัยกว่าเดิม
        private decimal? SafeConvertToDecimal(string value)
        {
            // ตรวจสอบค่า null, empty, หรือ whitespace
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // ทำความสะอาดข้อมูล
            var cleanValue = value.Trim();

            // ตรวจสอบ special values
            if (cleanValue.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                cleanValue.Equals("n/a", StringComparison.OrdinalIgnoreCase) ||
                cleanValue.Equals("na", StringComparison.OrdinalIgnoreCase) ||
                cleanValue.Equals("-", StringComparison.OrdinalIgnoreCase) ||
                cleanValue.Equals("", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            try
            {
                // ลองแปลงด้วย invariant culture ก่อน
                if (decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                {
                    return result;
                }

                // ลองแปลงด้วย current culture
                if (decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
                {
                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 Error converting '{cleanValue}' to decimal: {ex.Message}");
                return null;
            }
        }

        [HttpGet("GetAllProduct")]
        public async Task<IActionResult> ProductDropDownList()
        {
            try
            {
                var sizes = await _sqlcontext.ThRecords
                    .Where(t => !string.IsNullOrWhiteSpace(t.ImobileSize))
                    .Select(t => t.ImobileSize.Trim())
                    .Distinct()
                    .ToListAsync();

                return Ok(sizes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 Error fetching product sizes: " + ex.Message);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving product sizes.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("GetLotsBySize")]
        public async Task<IActionResult> GetLotsGroupedBySize()
        {
            try
            {
                var data = await _sqlcontext.ThRecords
                    .Where(r => !string.IsNullOrWhiteSpace(r.ImobileSize) && !string.IsNullOrWhiteSpace(r.ImobileLot))
                    .GroupBy(r => r.ImobileSize.Trim())
                    .Select(g => new
                    {
                        ImobileSize = g.Key,
                        ImobileLots = g.Select(x => x.ImobileLot.Trim()).Distinct().ToList()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Success",
                    data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 Error fetching lots by size: " + ex.Message);
                return StatusCode(500, new { message = "Error fetching data", error = ex.Message });
            }
        }

        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail()
        {
            try
            {
                var testBody = GenerateTestEmailBody();
                var subject = $"Test Email - Polishing System - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                await SendEmail(subject, testBody); // ✅ ไม่ต้องส่ง emailRecipientsService

                return Ok(new
                {
                    Success = true,
                    Message = "Test email sent successfully",
                    SmtpHost = _smtpSettings.Host,
                    Recipients = new
                    {
                        To = _emailRecipients.To,
                        Cc = _emailRecipients.Cc,
                        Bcc = _emailRecipients.Bcc
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("send-sample-report")]
        public async Task<IActionResult> SendSampleReport()
        {
            try
            {
                var currentTime = DateTime.Now;
                var fromTime = currentTime.AddHours(-6);

                // กำหนดช่วงเวลาและชื่อรอบ
                var timeSlot = GetTimeSlotName(currentTime.Hour);

                // ดึงข้อมูลจริงจาก database ย้อนหลัง 6 ชั่วโมง
                // แปลง TimeProcess เป็น DateTime โดยรวมกับ DateProcess
                var records = await _sqlcontext.ThRecords
                    .Where(th => th.ImobileType == "Polishing" &&
                                (th.Result == "Rescreen" || th.Result == "Hold" || th.Result == "Scrap" || th.Result == "RESCREEN"))
                    .ToListAsync(); // ดึงข้อมูลทั้งหมดมาก่อน

                // กรองข้อมูลตามช่วงเวลาใน memory
                var filteredRecords = records
                    .Where(th =>
                    {
                        // ดึงแค่ส่วนเวลาจาก TimeProcess แล้ว Add เข้ากับ DateProcess
                        var recordDateTime = th.DateProcess.Add(th.TimeProcess.TimeOfDay);
                        return recordDateTime >= fromTime && recordDateTime <= currentTime;
                    })
                    .OrderByDescending(th => th.DateProcess.Add(th.TimeProcess.TimeOfDay))
                    .ToList();

                // ✅ เพิ่มการเช็ค Th100Record สำหรับ Rescreen
                var enrichedRecords = new List<dynamic>();

                foreach (var record in filteredRecords)
                {
                    bool isRescreenCompleted = false;

                    // เช็คว่าเป็น Rescreen หรือไม่
                    if (record.Result?.ToUpper() == "RESCREEN")
                    {
                        // หาข้อมูลใน Th100Record ที่ตรงกัน
                        var th100Record = await _sqlcontext.Th100Records
                            .FirstOrDefaultAsync(th100 =>
                                th100.LotPo == record.LotPo &&
                                th100.McPo == record.McPo &&
                                th100.NoPo == record.NoPo);

                        if (th100Record != null)
                        {
                            isRescreenCompleted = true;
                        }
                    }

                    // สร้าง object ใหม่ที่มีข้อมูลเพิ่มเติม
                    enrichedRecords.Add(new
                    {
                        LotId = record.LotId,
                        LotPo = record.LotPo,
                        McPo = record.McPo,
                        NoPo = record.NoPo,
                        McType = record.McType,
                        Result = record.Result,
                        Remark = record.Remark,
                        DateProcess = record.DateProcess,
                        TimeProcess = record.TimeProcess,
                        ImobileLot = record.ImobileLot,
                        ImobileSize = record.ImobileSize,
                        Process = record.Process,
                        Status = record.Status,
                        IsRescreenCompleted = isRescreenCompleted // ✅ เพิ่มข้อมูลใหม่
                    });
                }

                string emailBody;
                string subject;

                if (enrichedRecords.Any())
                {
                    emailBody = GenerateEmailBodyWithRescreenStatus(enrichedRecords, currentTime, fromTime, currentTime);
                    subject = $"Polishing Alert Report - {timeSlot} {currentTime:yyyy-MM-dd HH:mm} ({enrichedRecords.Count} items)";
                }
                else
                {
                    emailBody = GenerateEmptyReportBodyUpdated(currentTime, fromTime, currentTime); // ✅ ใช้ method ใหม่
                    subject = $"Polishing Report - {timeSlot} {currentTime:yyyy-MM-dd HH:mm} (No Issues)";
                }

                await SendEmail(subject, emailBody);

                return Ok(new
                {
                    Success = true,
                    Message = $"Sample report sent successfully for {timeSlot}",
                    RecordCount = enrichedRecords.Count,
                    TimeSlot = timeSlot,
                    Period = $"{fromTime:yyyy-MM-dd HH:mm} - {currentTime:yyyy-MM-dd HH:mm}",
                    QueryPeriod = GetQueryPeriodDescription(fromTime, currentTime),
                    RescreenCompletedCount = enrichedRecords.Count(r => r.IsRescreenCompleted) // ✅ เพิ่มข้อมูลสถิติ
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ✅ สร้าง method ใหม่สำหรับ generate email body ที่รองรับ Rescreen status
        private string GenerateEmailBodyWithRescreenStatus(IEnumerable<dynamic> records, DateTime reportTime, DateTime fromTime, DateTime toTime)
        {
            var sb = new StringBuilder();
            int total = records.Count();
            int rescreenOk  = records.Cast<dynamic>().Count(r => (r.Result ?? "").ToString().ToUpper() == "RESCREEN" && r.IsRescreenCompleted == true);
            int rescreenPending = records.Cast<dynamic>().Count(r => (r.Result ?? "").ToString().ToUpper() == "RESCREEN" && r.IsRescreenCompleted == false);
            int hold  = records.Cast<dynamic>().Count(r => (r.Result ?? "").ToString().ToUpper() == "HOLD");
            int scrap = records.Cast<dynamic>().Count(r => (r.Result ?? "").ToString().ToUpper() == "SCRAP");

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'>");
            sb.AppendLine("<style>");
            sb.AppendLine("  body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 20px; background: #f5f5f5; color: #333; }");
            sb.AppendLine("  .container { max-width: 760px; margin: 0 auto; background: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }");
            sb.AppendLine("  .header { background: linear-gradient(135deg, #c62828, #e53935); padding: 24px 32px; color: #fff; }");
            sb.AppendLine("  .header h1 { margin: 0 0 4px; font-size: 22px; font-weight: 700; letter-spacing: -0.3px; }");
            sb.AppendLine("  .header p { margin: 0; font-size: 13px; opacity: 0.85; }");
            sb.AppendLine("  .summary { display: flex; gap: 12px; padding: 20px 32px; background: #fafafa; border-bottom: 1px solid #eee; }");
            sb.AppendLine("  .stat { flex: 1; text-align: center; padding: 12px 8px; border-radius: 8px; }");
            sb.AppendLine("  .stat-num { font-size: 28px; font-weight: 700; line-height: 1; }");
            sb.AppendLine("  .stat-lbl { font-size: 11px; text-transform: uppercase; letter-spacing: 0.05em; margin-top: 4px; opacity: 0.7; }");
            sb.AppendLine("  .stat-total   { background: #fff3e0; color: #e65100; }");
            sb.AppendLine("  .stat-rescreen{ background: #ffebee; color: #c62828; }");
            sb.AppendLine("  .stat-ok      { background: #e8f5e9; color: #2e7d32; }");
            sb.AppendLine("  .stat-hold    { background: #e3f2fd; color: #1565c0; }");
            sb.AppendLine("  .stat-scrap   { background: #fce4ec; color: #880e4f; }");
            sb.AppendLine("  .content { padding: 24px 32px; }");
            sb.AppendLine("  table { width: 100%; border-collapse: collapse; font-size: 13px; }");
            sb.AppendLine("  thead tr { background: #f5f5f5; }");
            sb.AppendLine("  th { padding: 10px 12px; text-align: left; font-weight: 700; font-size: 11px; text-transform: uppercase; letter-spacing: 0.06em; color: #666; border-bottom: 2px solid #e0e0e0; }");
            sb.AppendLine("  th.center, td.center { text-align: center; }");
            sb.AppendLine("  td { padding: 10px 12px; border-bottom: 1px solid #f0f0f0; vertical-align: middle; }");
            sb.AppendLine("  tr:last-child td { border-bottom: none; }");
            sb.AppendLine("  .badge { display: inline-block; padding: 3px 10px; border-radius: 20px; font-size: 11px; font-weight: 700; }");
            sb.AppendLine("  .badge-rescreen { background: #ffebee; color: #c62828; }");
            sb.AppendLine("  .badge-hold     { background: #e3f2fd; color: #1565c0; }");
            sb.AppendLine("  .badge-scrap    { background: #fce4ec; color: #880e4f; }");
            sb.AppendLine("  .badge-ok       { background: #e8f5e9; color: #2e7d32; }");
            sb.AppendLine("  .footer { padding: 16px 32px; background: #f9f9f9; border-top: 1px solid #eee; font-size: 11px; color: #999; }");
            sb.AppendLine("</style></head>");
            sb.AppendLine("<body><div class='container'>");

            // Header
            sb.AppendLine($"<div class='header'>");
            sb.AppendLine($"  <h1>Polishing Alert Report</h1>");
            sb.AppendLine($"  <p>Period: {fromTime:yyyy-MM-dd HH:mm} &ndash; {reportTime:yyyy-MM-dd HH:mm}</p>");
            sb.AppendLine("</div>");

            // Summary stats
            sb.AppendLine("<div class='summary'>");
            sb.AppendLine($"  <div class='stat stat-total'><div class='stat-num'>{total}</div><div class='stat-lbl'>Total</div></div>");
            sb.AppendLine($"  <div class='stat stat-rescreen'><div class='stat-num'>{rescreenPending}</div><div class='stat-lbl'>Rescreen</div></div>");
            sb.AppendLine($"  <div class='stat stat-ok'><div class='stat-num'>{rescreenOk}</div><div class='stat-lbl'>Rescreen OK</div></div>");
            sb.AppendLine($"  <div class='stat stat-hold'><div class='stat-num'>{hold}</div><div class='stat-lbl'>Hold</div></div>");
            sb.AppendLine($"  <div class='stat stat-scrap'><div class='stat-num'>{scrap}</div><div class='stat-lbl'>Scrap</div></div>");
            sb.AppendLine("</div>");

            // Table
            sb.AppendLine("<div class='content'>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("  <th class='center' style='width:44px'>No.</th>");
            sb.AppendLine("  <th>PO Lot</th>");
            sb.AppendLine("  <th class='center' style='width:60px'>MC</th>");
            sb.AppendLine("  <th class='center'>Status Thickness</th>");
            sb.AppendLine("  <th class='center'>Status</th>");
            sb.AppendLine("</tr></thead><tbody>");

            int rowNumber = 1;
            foreach (var record in records)
            {
                string poLot   = $"{record.LotPo ?? ""}{record.McPo ?? ""}{record.NoPo ?? ""}";
                string mc      = record.McPo ?? "";
                string status  = (record.Result ?? "").ToString().ToUpper();
                bool rescreenDone = record.IsRescreenCompleted == true;

                string bgColor = status switch
                {
                    "RESCREEN" when rescreenDone => "#f1f8e9",
                    "RESCREEN"                   => "#fff8f8",
                    "HOLD"                       => "#f3f8ff",
                    "SCRAP"                      => "#fff0f3",
                    _                            => "#ffffff"
                };

                string badgeClass = status switch
                {
                    "RESCREEN" => "badge-rescreen",
                    "HOLD"     => "badge-hold",
                    "SCRAP"    => "badge-scrap",
                    _          => ""
                };

                string finalStatus = "";
                if (status == "RESCREEN" && rescreenDone)
                    finalStatus = "<span class='badge badge-ok'>Rescreen OK</span>";

                sb.AppendLine($"<tr style='background:{bgColor}'>");
                sb.AppendLine($"  <td class='center' style='color:#999;font-size:12px'>{rowNumber}</td>");
                sb.AppendLine($"  <td style='font-family:monospace;font-size:12px'>{poLot}</td>");
                sb.AppendLine($"  <td class='center'>{mc}</td>");
                sb.AppendLine($"  <td class='center'><span class='badge {badgeClass}'>{status}</span></td>");
                sb.AppendLine($"  <td class='center'>{finalStatus}</td>");
                sb.AppendLine("</tr>");
                rowNumber++;
            }

            sb.AppendLine("</tbody></table></div>");

            // Footer
            sb.AppendLine($"<div class='footer'>Generated at {DateTime.Now:yyyy-MM-dd HH:mm:ss} &bull; Polishing Alert System &bull; This is an automated email</div>");
            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        private string GenerateEmptyReportBodyUpdated(DateTime reportTime, DateTime fromTime, DateTime toTime)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; margin: 20px;'>");

            // Header
            sb.AppendLine($"<h2 style='color: #4caf50;'>✅ Polishing Report - All Clear</h2>");
            sb.AppendLine($"<p><strong>Report Time:</strong> {reportTime:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Period:</strong> {fromTime:yyyy-MM-dd HH:mm} - {toTime:yyyy-MM-dd HH:mm}</p>");

            // Success Message
            sb.AppendLine("<div style='margin: 20px 0; padding: 20px; background-color: #e8f5e8; border-radius: 5px; border-left: 5px solid #4caf50;'>");
            sb.AppendLine("<p style='color: #4caf50; font-size: 18px; margin: 0;'><strong>🎉 ไม่พบปัญหาใด ๆ ในช่วงเวลานี้</strong></p>");
            sb.AppendLine("<p style='margin: 10px 0 0 0; color: #666;'>ระบบ Polishing ทำงานปกติ ไม่มี Rescreen, Hold หรือ Scrap</p>");
            sb.AppendLine("</div>");

            // Empty Table for consistency
            sb.AppendLine("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%; margin-top: 20px;'>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr style='background-color: #f5f5f5;'>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>No.</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>Status Thickness</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>PO Lot</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>MC</th>");
            // Status Rescreen column removed
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>Status</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td colspan='5' style='text-align: center; padding: 20px; color: #666; font-style: italic;'>No issues found in this period</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Footer
            sb.AppendLine("<hr style='margin-top: 30px;'>");
            sb.AppendLine("<p style='color: #666; font-size: 12px;'>");
            sb.AppendLine("This is an automated report from Polishing System.<br>");
            sb.AppendLine($"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("</p>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        // เพิ่ม method สำหรับกำหนดชื่อรอบเวลา
        private string GetTimeSlotName(int hour)
        {
            return hour switch
            {
                >= 6 and < 12 => "Morning Shift (06:00-12:00)",
                >= 12 and < 18 => "Day Shift (12:00-18:00)",
                >= 18 and < 24 => "Evening Shift (18:00-24:00)",
                >= 0 and < 6 => "Night Shift (00:00-06:00)",
                _ => "Unknown Shift"
            };
        }

        // เพิ่ม method สำหรับแสดงช่วงเวลาที่ query
        private string GetQueryPeriodDescription(DateTime fromTime, DateTime toTime)
        {
            if (fromTime.Date == toTime.Date)
            {
                return $"Same day: {fromTime:HH:mm} - {toTime:HH:mm}";
            }
            else
            {
                return $"Cross day: {fromTime:yyyy-MM-dd HH:mm} - {toTime:yyyy-MM-dd HH:mm}";
            }
        }

        private string GenerateTestEmailBody()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head><meta charset='UTF-8'></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; margin: 20px;'>");
            sb.AppendLine("<h2 style='color: #2196f3;'>📧 Email Configuration Test</h2>");
            sb.AppendLine($"<p><strong>Test Time:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>SMTP Host:</strong> {_smtpSettings.Host}:{_smtpSettings.Port}</p>");
            sb.AppendLine($"<p><strong>From:</strong> {_smtpSettings.FromEmail}</p>");
            sb.AppendLine("<p style='color: #4caf50; font-size: 16px;'><strong>✅ Email system is working correctly!</strong></p>");
            sb.AppendLine("<p>The Polishing Alert System email service has been configured and tested successfully.</p>");
            sb.AppendLine("<hr><p><small>Test Email - Polishing System</small></p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private string GenerateEmailBody(IEnumerable<dynamic> records, DateTime reportTime, DateTime fromTime, DateTime toTime)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; margin: 20px;'>");

            // Header
            sb.AppendLine($"<h2 style='color: #d32f2f;'>🚨 Polishing Alert Report</h2>");
            sb.AppendLine($"<p><strong>Report Time:</strong> {reportTime:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Period:</strong> {fromTime:yyyy-MM-dd HH:mm} - {toTime:yyyy-MM-dd HH:mm}</p>");
            sb.AppendLine($"<p><strong>Total Issues:</strong> {records.Count()} รายการ</p>");

            // Table
            sb.AppendLine("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%; margin-top: 20px;'>");

            // Table Header
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr style='background-color: #f5f5f5;'>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>No.</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>Status Thickness</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>PO Lot</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>MC</th>");
            // Status Rescreen column removed
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");

            // Table Body
            sb.AppendLine("<tbody>");
            int rowNumber = 1;

            foreach (var record in records)
            {
                // สร้าง PO-Lot จาก LotPo + McPo + NoPo
                string poLot = $"{record.LotPo ?? ""}{record.McPo ?? ""}{record.NoPo ?? ""}";

                // MC คือ McPo
                string mc = record.McPo ?? "";

                // Status Thickness คือ Result
                string statusThickness = record.Result ?? "";

                // กำหนดสีพื้นหลังตาม Status
                string bgColor = statusThickness.ToUpper() switch
                {
                    "RESCREEN" => "#ffebee", // สีแดงอ่อน
                    "HOLD" => "#fff3e0",     // สีส้มอ่อน
                    "SCRAP" => "#ffebee",    // สีแดงอ่อน
                    _ => "#ffffff"           // สีขาว
                };

                sb.AppendLine($"<tr style='background-color: {bgColor};'>");
                sb.AppendLine($"<td style='text-align: center;'>{rowNumber}</td>");
                sb.AppendLine($"<td style='text-align: center;'>{statusThickness}</td>");
                sb.AppendLine($"<td style='text-align: center;'>{poLot}</td>");
                sb.AppendLine($"<td style='text-align: center;'>{mc}</td>");
                // Link td removed
                sb.AppendLine("</tr>");

                rowNumber++;
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Additional Info
            sb.AppendLine("<div style='margin-top: 20px; padding: 15px; background-color: #f0f0f0; border-radius: 5px;'>");
            sb.AppendLine("<h3 style='margin-top: 0; color: #333;'>📊 Summary</h3>");

            // นับจำนวนแต่ละ Status
            var statusCounts = records.GroupBy(r => r.Result ?? "Unknown")
                                     .ToDictionary(g => g.Key, g => g.Count());

            foreach (var status in statusCounts)
            {
                sb.AppendLine($"<p><strong>{status.Key}:</strong> {status.Value} รายการ</p>");
            }
            sb.AppendLine("</div>");

            // Footer
            sb.AppendLine("<hr style='margin-top: 30px;'>");
            sb.AppendLine("<p style='color: #666; font-size: 12px;'>");
            sb.AppendLine("This is an automated alert from Polishing System.<br>");
            sb.AppendLine($"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("</p>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private string GenerateEmptyReportBody(DateTime reportTime, DateTime fromTime, DateTime toTime)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; margin: 20px;'>");

            // Header
            sb.AppendLine($"<h2 style='color: #4caf50;'>✅ Polishing Report - All Clear</h2>");
            sb.AppendLine($"<p><strong>Report Time:</strong> {reportTime:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Period:</strong> {fromTime:yyyy-MM-dd HH:mm} - {toTime:yyyy-MM-dd HH:mm}</p>");

            // Success Message
            sb.AppendLine("<div style='margin: 20px 0; padding: 20px; background-color: #e8f5e8; border-radius: 5px; border-left: 5px solid #4caf50;'>");
            sb.AppendLine("<p style='color: #4caf50; font-size: 18px; margin: 0;'><strong>🎉 ไม่พบปัญหาใด ๆ ในช่วงเวลานี้</strong></p>");
            sb.AppendLine("<p style='margin: 10px 0 0 0; color: #666;'>ระบบ Polishing ทำงานปกติ ไม่มี Rescreen, Hold หรือ Scrap</p>");
            sb.AppendLine("</div>");

            // Empty Table for consistency
            sb.AppendLine("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%; margin-top: 20px;'>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr style='background-color: #f5f5f5;'>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>No.</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>Status Thickness</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>PO Lot</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>MC</th>");
            // Status Rescreen column removed
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td colspan='5' style='text-align: center; padding: 20px; color: #666; font-style: italic;'>No issues found in this period</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Footer
            sb.AppendLine("<hr style='margin-top: 30px;'>");
            sb.AppendLine("<p style='color: #666; font-size: 12px;'>");
            sb.AppendLine("This is an automated report from Polishing System.<br>");
            sb.AppendLine($"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("</p>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }


        // ✅ แก้ไข SendEmail method ให้ใช้ _emailRecipients โดยตรง
        private async Task SendEmail(string subject, string body)
        {
            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port);
            smtpClient.EnableSsl = _smtpSettings.EnableSsl;
            smtpClient.UseDefaultCredentials = _smtpSettings.UseDefaultCredentials;

            if (!_smtpSettings.UseDefaultCredentials)
            {
                smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
            }

            using var mailMessage = new MailMessage();
            var fromAddress = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName);
            mailMessage.From = fromAddress;

            // ✅ ใช้ _emailRecipients โดยตรง
            foreach (var email in _emailRecipients.To)
            {
                if (!string.IsNullOrWhiteSpace(email))
                    mailMessage.To.Add(email.Trim());
            }

            foreach (var email in _emailRecipients.Cc)
            {
                if (!string.IsNullOrWhiteSpace(email))
                    mailMessage.CC.Add(email.Trim());
            }

            foreach (var email in _emailRecipients.Bcc)
            {
                if (!string.IsNullOrWhiteSpace(email))
                    mailMessage.Bcc.Add(email.Trim());
            }

            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;

            await smtpClient.SendMailAsync(mailMessage);
        }
        [HttpGet("service-status")]
        public IActionResult GetServiceStatus()
        {
            try
            {
                // ดึง EmailScheduleService จาก DI
                var emailService = HttpContext.RequestServices
                    .GetService<EmailScheduleService>();

                if (emailService == null)
                {
                    return BadRequest(new
                    {
                        Status = "Not Found",
                        Message = "EmailScheduleService not registered"
                    });
                }

                var status = emailService.GetServiceStatus();

                return Ok(new
                {
                    Status = "OK",
                    ServiceStatus = status,
                    IsEnabled = status.IsEnabled,
                    CurrentTime = status.CurrentTime,
                    LastSentTime = status.LastSentTime,
                    NextScheduledTimes = status.NextScheduledTimes,
                    Configuration = status.Configuration
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = ex.Message
                });
            }
        }
        [HttpGet("schedule-status")]
        public IActionResult GetScheduleStatus()
        {
            try
            {
                var hostedServices = HttpContext.RequestServices.GetServices<IHostedService>();
                var emailService = hostedServices.OfType<EmailScheduleService>().FirstOrDefault();

                if (emailService == null)
                {
                    return BadRequest(new
                    {
                        Status = "Not Found",
                        Message = "EmailScheduleService not found in hosted services",
                        HostedServicesCount = hostedServices.Count(),
                        Services = hostedServices.Select(s => s.GetType().Name).ToList()
                    });
                }

                // หาการ get status ได้ต้องเรียกผ่าน Service Provider
                var serviceProvider = HttpContext.RequestServices;
                var scheduleService = serviceProvider.GetService<EmailScheduleService>();

                return Ok(new
                {
                    Status = "Running",
                    ServiceFound = true,
                    CurrentTime = DateTime.Now,
                    Message = "EmailScheduleService is registered and running"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = ex.Message,
                    Exception = ex.ToString()
                });
            }
        }
    }
}