using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Text;
using SI24004.Models;
using SI24004.Service;
using SI24004.Models.Requests;
using SI24004.ModelsSQL;

namespace SI24004.Service
{
    // EmailRecipients Service
    public class EmailRecipientsService : IEmailRecipientsService
    {
        private readonly IOptions<EmailRecipients> _emailRecipients;
        private readonly ILogger<EmailRecipientsService> _logger;
        public EmailRecipientsService(
            IOptions<EmailRecipients> emailRecipients,
            ILogger<EmailRecipientsService> logger)
        {
            _emailRecipients = emailRecipients;
            _logger = logger;
        }
        public EmailRecipients GetAllRecipients()
        {
            try
            {
                var recipients = _emailRecipients.Value;
                if (recipients == null)
                {
                    _logger.LogWarning("EmailRecipients configuration is null, using default");
                    return new EmailRecipients
                    {
                        To = new List<string> { "anupong.ohok@agc.com" },
                        Cc = new List<string>(),
                        Bcc = new List<string>()
                    };
                }
                return recipients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email recipients, using default");
                return new EmailRecipients
                {
                    To = new List<string> { "anupong.ohok@agc.com" },
                    Cc = new List<string>(),
                    Bcc = new List<string>()
                };
            }
        }
    }
    // EmailRecipients class สำหรับจัดการรายชื่อผู้รับ
    public class EmailRecipients
    {
        public List<string> To { get; set; } = new();
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
    }
    public class EnrichedRecord
    {
        public object Record { get; set; }
        public bool IsRescreenCompleted { get; set; }
    }
    public class EmailScheduleService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailScheduleService> _logger;
        private readonly SmtpSettings _smtpSettings;
        private readonly ScheduleSettings _scheduleSettings; // ✅ เพิ่ม Schedule Settings
        private readonly TimeZoneInfo _thailandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        private DateTime _lastSentTime = DateTime.MinValue; // ✅ เก็บเวลาส่งครั้งล่าสุด
        private readonly Dictionary<int, DateTime> _lastSentByHour = new(); // ✅ ป้องกันส่งซ้ำแต่ละชั่วโมง

        public EmailScheduleService(
            IServiceProvider serviceProvider,
            ILogger<EmailScheduleService> logger,
            IOptions<SmtpSettings> smtpSettings,
            IOptions<ScheduleSettings> scheduleSettings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // ✅ ป้องกัน null configuration
            _smtpSettings = smtpSettings?.Value ?? throw new ArgumentNullException(nameof(smtpSettings));
            _scheduleSettings = scheduleSettings?.Value ?? throw new ArgumentNullException(nameof(scheduleSettings));

            // ✅ ป้องกัน TimeZone error
            try
            {
                _thailandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (Exception)
            {
                // Fallback สำหรับ Linux/Mac
                try
                {
                    _thailandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
                }
                catch (Exception)
                {
                    _logger.LogWarning("Cannot find Thailand timezone, using UTC+7 offset");
                    _thailandTimeZone = TimeZoneInfo.CreateCustomTimeZone("Thailand", TimeSpan.FromHours(7), "Thailand", "Thailand");
                }
            }
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚀 EmailScheduleService Starting...");
            _logger.LogInformation("Configuration:");
            _logger.LogInformation("- EnableSchedule: {EnableSchedule}", _scheduleSettings.EnableSchedule);
            _logger.LogInformation("- ScheduleHours: [{Hours}]", string.Join(", ", _scheduleSettings.ScheduleHours));
            _logger.LogInformation("- CheckInterval: {Interval}s", _scheduleSettings.CheckIntervalSeconds);
            _logger.LogInformation("- DataLookback: {Hours}h", _scheduleSettings.DataLookbackHours);
            _logger.LogInformation("- SMTP: {Host}:{Port}", _smtpSettings.Host, _smtpSettings.Port);

            await base.StartAsync(cancellationToken);
            _logger.LogInformation("✅ EmailScheduleService Started Successfully");
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 EmailScheduleService Stopping...");
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("✅ EmailScheduleService Stopped");
        }
        private DateTime GetThailandTime()
        {
            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _thailandTimeZone);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error converting to Thailand time, using UTC+7");
                return DateTime.UtcNow.AddHours(7);
            }
        }
        private bool IsScheduledTime(int hour)
        {
            return _scheduleSettings.ScheduleHours.Contains(hour);
        }
        private bool ShouldSendEmail(DateTime currentTime)
        {
            var currentHour = currentTime.Hour;
            var currentMinute = currentTime.Minute;

            // ต้องเป็นชั่วโมงที่กำหนดและนาทีต้องเป็น 0
            if (!IsScheduledTime(currentHour) || currentMinute != 0)
                return false;

            // ตรวจสอบว่าส่งไปแล้วในชั่วโมงนี้หรือยัง
            if (_lastSentByHour.ContainsKey(currentHour))
            {
                var lastSentForThisHour = _lastSentByHour[currentHour];
                var timeDiff = currentTime - lastSentForThisHour;

                // ถ้าส่งไปแล้วในชั่วโมงนี้และยังไม่ครบ 50 นาที ไม่ต้องส่งอีก
                if (timeDiff.TotalMinutes < 50)
                {
                    return false;
                }
            }

            return true;
        }
        private async Task<bool> SendScheduledEmailWithRetry(DateTime currentTime)
        {
            for (int attempt = 1; attempt <= _scheduleSettings.MaxRetryAttempts; attempt++)
            {
                try
                {
                    _logger.LogInformation("📧 Sending email attempt {Attempt}/{MaxAttempts}",
                        attempt, _scheduleSettings.MaxRetryAttempts);

                    await SendScheduledEmail(currentTime);
                    _logger.LogInformation("✅ Email sent successfully on attempt {Attempt}", attempt);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Email sending failed on attempt {Attempt}/{MaxAttempts}: {Error}",
                        attempt, _scheduleSettings.MaxRetryAttempts, ex.Message);

                    if (attempt < _scheduleSettings.MaxRetryAttempts)
                    {
                        _logger.LogInformation("⏳ Waiting {Delay}s before retry...", _scheduleSettings.RetryDelaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(_scheduleSettings.RetryDelaySeconds));
                    }
                }
            }

            return false;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Validation
            if (!_scheduleSettings.EnableSchedule)
            {
                _logger.LogInformation("📴 EmailScheduleService Disabled - Exiting");
                return;
            }

            if (_scheduleSettings.ScheduleHours == null || !_scheduleSettings.ScheduleHours.Any())
            {
                _logger.LogError("❌ No schedule hours configured - Service cannot run");
                return;
            }

            _logger.LogInformation("⚡ EmailScheduleService Main Loop Started");

            // Main Loop
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var currentThailandTime = GetThailandTime();
                    var currentHour = currentThailandTime.Hour;
                    var currentMinute = currentThailandTime.Minute;

                    // ✅ Log status ทุก 10 นาที
                    if (currentMinute % 10 == 0 && currentMinute < 2) // ป้องกัน log spam
                    {
                        _logger.LogInformation("🕐 Status Check - Time: {Time}, IsScheduledHour: {IsScheduled}, LastSent: {LastSent}",
                            currentThailandTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            IsScheduledTime(currentHour),
                            _lastSentTime == DateTime.MinValue ? "Never" : _lastSentTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    }

                    // ✅ ตรวจสอบเงื่อนไขการส่งเมล
                    if (ShouldSendEmail(currentThailandTime))
                    {
                        _logger.LogInformation("⏰ Triggering scheduled email at {Time} for hour {Hour}",
                            currentThailandTime.ToString("yyyy-MM-dd HH:mm:ss"), currentHour);

                        var success = await SendScheduledEmailWithRetry(currentThailandTime);

                        if (success)
                        {
                            _lastSentTime = currentThailandTime;
                            _lastSentByHour[currentHour] = currentThailandTime;

                            _logger.LogInformation("✅ Email sent successfully, waiting {Delay}s before next check",
                                _scheduleSettings.DelayAfterSendSeconds);

                            await Task.Delay(TimeSpan.FromSeconds(_scheduleSettings.DelayAfterSendSeconds), stoppingToken);
                        }
                        else
                        {
                            _logger.LogError("❌ Failed to send email after all retry attempts");
                            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // รอ 5 นาทีก่อน retry
                        }
                    }
                    else
                    {
                        // รอตาม interval ปกติ
                        await Task.Delay(TimeSpan.FromSeconds(_scheduleSettings.CheckIntervalSeconds), stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("🏁 EmailScheduleService Cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "💥 Unexpected error in EmailScheduleService main loop");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // รอ 1 นาทีแล้วลองใหม่
                }
            }

            _logger.LogInformation("🏁 EmailScheduleService Main Loop Ended");
        }
        // ✅ เพิ่ม method ตรวจสอบว่าเพิ่งส่งไปหรือไม่ (ป้องกันส่งซ้ำ)
        private bool IsRecentlySent(DateTime currentTime)
        {
            if (_lastSentTime == DateTime.MinValue) return false;

            var timeDifference = currentTime - _lastSentTime;
            // ถ้าส่งไปแล้วไม่ถึง 50 นาที ให้ถือว่าเพิ่งส่งไป
            return timeDifference.TotalMinutes < 50;
        }
        private DateTime? CombineDateAndTime(DateTime? dateProcess, DateTime? timeProcess)
        {
            try
            {
                if (dateProcess == null || timeProcess == null)
                    return null;

                // Get only the date part from DateProcess and time part from TimeProcess
                var datePart = dateProcess.Value.Date;
                var timePart = timeProcess.Value.TimeOfDay;

                return datePart.Add(timePart);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error combining date and time");
                return null;
            }
        }
        // ✅ ปรับปรุง SendScheduledEmail ให้รับ parameter
        private async Task SendScheduledEmail(DateTime? currentTime = null)
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var sqlContext = scope.ServiceProvider.GetRequiredService<ThicknessContext>();
                var reportTime = currentTime ?? GetThailandTime();
                var fromTime = reportTime.AddHours(-_scheduleSettings.DataLookbackHours);
                var toTime = reportTime;

                _logger.LogInformation("?? Querying data from {FromTime} to {ToTime}",
                    fromTime.ToString("yyyy-MM-dd HH:mm"), toTime.ToString("yyyy-MM-dd HH:mm"));

                // Query database
                var records = await sqlContext.ThRecords
                    .Where(th => th.ImobileType == "Polishing" &&
                        (th.Result == "Rescreen" || th.Result == "Hold" || th.Result == "Scrap" || th.Result == "RESCREEN"))
                    .ToListAsync();

                _logger.LogInformation("?? Found {RecordCount} total records", records.Count);

                // Filter records by time range
                var filteredRecords = records
                    .Where(th =>
                    {
                        // Combine DateProcess and TimeProcess to get full DateTime
                        var recordDateTime = th.DateProcess.Add(th.TimeProcess.TimeOfDay);
                        return recordDateTime >= fromTime && recordDateTime <= toTime; // Fix: use toTime instead of currentTime
                    })
                    .OrderByDescending(th => th.DateProcess.Add(th.TimeProcess.TimeOfDay))
                    .ToList();

                _logger.LogInformation("?? Filtered to {FilteredCount} records in time range", filteredRecords.Count);

                // Check Rescreen status - Use filteredRecords instead of records
                var enrichedRecords = new List<(ThRecord record, bool isRescreenCompleted)>();

                if (filteredRecords?.Any() == true) // Add null check
                {
                    foreach (var record in filteredRecords) // Fix: use filteredRecords
                    {
                        bool isRescreenCompleted = false;

                        if (string.Equals(record.Result, "Rescreen", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(record.Result, "RESCREEN", StringComparison.OrdinalIgnoreCase))
                        {
                            var th100Record = await sqlContext.Th100Records
                                .FirstOrDefaultAsync(th100 =>
                                    th100.LotPo == record.LotPo &&
                                    th100.McPo == record.McPo &&
                                    th100.NoPo == record.NoPo);

                            isRescreenCompleted = th100Record != null;
                        }

                        enrichedRecords.Add((record, isRescreenCompleted));
                    }
                }

                _logger.LogInformation("? Enriched {EnrichedCount} records", enrichedRecords.Count);

                // Send email
                string emailBody, subject;
                var timeSlot = GetTimeSlotName(reportTime.Hour);

                if (enrichedRecords?.Any() == true) // Add null check
                {
                        emailBody = GenerateEnhancedEmailBody(enrichedRecords, reportTime, fromTime, toTime);
                        subject = $"Polishing Alert Report - {timeSlot} {reportTime:yyyy-MM-dd HH:mm} ({enrichedRecords.Count} items)";   
                }
                else if (_scheduleSettings.SendEmptyReports)
                {
                    emailBody = GenerateEmptyReportBody(reportTime, fromTime, toTime);
                    subject = $"Polishing Report - {timeSlot} {reportTime:yyyy-MM-dd HH:mm} (No Issues)";
                }
                else
                {
                    _logger.LogInformation("?? No records found and empty reports disabled - skipping email");
                    return;
                }

                await SendEmail(subject, emailBody, scope.ServiceProvider);
                _logger.LogInformation("?? Email sent successfully: {Subject}", subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error in SendScheduledEmail");

                if (_scheduleSettings.SendErrorNotifications)
                {
                    try
                    {
                        var errorBody = GenerateErrorReportBody(ex, DateTime.Now);
                        await SendEmail($"Polishing System Error - {DateTime.Now:yyyy-MM-dd HH:mm}",
                                      errorBody, scope.ServiceProvider);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "? Failed to send error notification");
                    }
                }
                throw; // Re-throw for retry logic
            }
        }
        private static (double avg, double max, double min, double diff) CalculateCaStatistics(ThRecord record)
        {
            var caValues = new List<double>();

            var caProperties = new[]
            {
                record.Ca1In1, record.Ca1In2, record.Ca1In3, record.Ca1In4, record.Ca1In5,
                record.Ca1Out1, record.Ca1Out2, record.Ca1Out3, record.Ca1Out4, record.Ca1Out5,
                record.Ca2In1, record.Ca2In2, record.Ca2In3, record.Ca2In4, record.Ca2In5,
                record.Ca2Out1, record.Ca2Out2, record.Ca2Out3, record.Ca2Out4, record.Ca2Out5,
                record.Ca3In1, record.Ca3In2, record.Ca3In3, record.Ca3In4, record.Ca3In5,
                record.Ca3Out1, record.Ca3Out2, record.Ca3Out3, record.Ca3Out4, record.Ca3Out5,
                record.Ca4In1, record.Ca4In2, record.Ca4In3, record.Ca4In4, record.Ca4In5,
                record.Ca4Out1, record.Ca4Out2, record.Ca4Out3, record.Ca4Out4, record.Ca4Out5,
                record.Ca5In1, record.Ca5In2, record.Ca5In3, record.Ca5In4, record.Ca5In5,
                record.Ca5Out1, record.Ca5Out2, record.Ca5Out3, record.Ca5Out4, record.Ca5Out5
            };
            foreach (var prop in caProperties)
            {
                if (prop != null && !string.IsNullOrWhiteSpace(prop.ToString()))
                {
                    if (double.TryParse(prop.ToString(), out double value) && value > 0)
                    {
                        caValues.Add(value);
                    }
                }
            }

            if (!caValues.Any())
            {
                return (0, 0, 0, 0);
            }

            var avg = caValues.Average();
            var max = caValues.Max();
            var min = caValues.Min();
            var diff = min - max;

            return (avg, max, min, diff);
        }
        // ✅ ปรับปรุง Email Body Generation
        private string GenerateEnhancedEmailBody(IEnumerable<(ThRecord record, bool isRescreenCompleted)> records, DateTime reportTime, DateTime fromTime, DateTime toTime)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; margin: 20px;'>");

            // Header
            sb.AppendLine($"<h2 style='color: #d32f2f;'>Polishing Alert Report</h2>");
            sb.AppendLine($"<p><strong>Report Time:</strong> {reportTime:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Period:</strong> {fromTime:yyyy-MM-dd HH:mm} - {toTime:yyyy-MM-dd HH:mm}</p>");
            sb.AppendLine($"<p><strong>Total Issues:</strong> {records.Count()} </p>");

            // Table with enhanced columns
            sb.AppendLine("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%; margin-top: 20px; font-size: 12px;'>");

            // Table Header
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr style='background-color: #f5f5f5;'>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>No.</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>Status Thickness</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>PO Lot</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>MC</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold; background-color: #e3f2fd;'>Avg</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold; background-color: #ffebee;'>Max</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold; background-color: #e8f5e8;'>Min</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold; background-color: #fff3e0;'>Diff</th>");
            sb.AppendLine("<th style='text-align: center; font-weight: bold;'>Status</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");

            // Table Body
            sb.AppendLine("<tbody>");
            int rowNumber = 1;

            foreach (var item in records)
            {
                var record = item.record;
                var isRescreenCompleted = item.isRescreenCompleted;

                // ???????? Ca statistics
                var (avg, max, min, diff) = CalculateCaStatistics(record);

                // ????? PO-Lot ??? LotPo + McPo + NoPo
                string poLot = $"{record.LotPo ?? ""}{record.McPo ?? ""}{record.NoPo ?? ""}";
                string mc = record.McPo ?? "";
                string statusThickness = record.Result ?? "";

                // Status ?????? Rescreen
                string finalStatus = "";
                if (statusThickness.ToUpper() == "RESCREEN" && isRescreenCompleted)
                {
                    finalStatus = "<span style='color: #4caf50; font-weight: bold;'>Rescreen OK</span>";
                }

                // Background color ??? Status
                string bgColor = statusThickness.ToUpper() switch
                {
                    "RESCREEN" when isRescreenCompleted => "#e8f5e8",
                    "RESCREEN" => "#ffebee",
                    "HOLD" => "#fff3e0",
                    "SCRAP" => "#ffebee",
                    _ => "#ffffff"
                };

                sb.AppendLine($"<tr style='background-color: {bgColor};'>");
                sb.AppendLine($"<td style='text-align: center;'>{rowNumber}</td>");
                sb.AppendLine($"<td style='text-align: center;'>{statusThickness}</td>");
                sb.AppendLine($"<td style='text-align: center;'>{poLot}</td>");
                sb.AppendLine($"<td style='text-align: center;'>{mc}</td>");

                // ??????? Avg, Max, Min, Diff
                sb.AppendLine($"<td style='text-align: center; background-color: #f8f9ff;'>{(avg > 0 ? avg.ToString("F2") : "-")}</td>");
                sb.AppendLine($"<td style='text-align: center; background-color: #fff5f5;'>{(max > 0 ? max.ToString("F2") : "-")}</td>");
                sb.AppendLine($"<td style='text-align: center; background-color: #f5fff5;'>{(min > 0 ? min.ToString("F2") : "-")}</td>");
                sb.AppendLine($"<td style='text-align: center; background-color: #fffaf0;'>{(avg > 0 ? diff.ToString("F2") : "-")}</td>");

                sb.AppendLine($"<td style='text-align: center;'>{finalStatus}</td>");
                sb.AppendLine("</tr>");

                rowNumber++;
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Additional Info with Statistics Summary
            sb.AppendLine("<div style='margin-top: 20px; padding: 15px; background-color: #f0f0f0; border-radius: 5px;'>");
            sb.AppendLine("<h3 style='margin-top: 0; color: #333;'>Summary</h3>");

            // Status counts
            var statusCounts = records.GroupBy(r => r.record.Result ?? "Unknown")
                                     .ToDictionary(g => g.Key, g => g.Count());

            foreach (var status in statusCounts)
            {
                if (status.Key.ToUpper() == "RESCREEN")
                {
                    var rescreenRecords = records.Where(r => (r.record.Result ?? "").ToUpper() == "RESCREEN").ToList();
                    var completedCount = rescreenRecords.Count(r => r.isRescreenCompleted);
                    var pendingCount = rescreenRecords.Count() - completedCount;

                    sb.AppendLine($"<p><strong>{status.Key}:</strong> {status.Value} ");
                    sb.AppendLine($"<span style='margin-left: 20px; color: #4caf50;'>Completed: {completedCount}</span>");
                    sb.AppendLine($"<span style='margin-left: 10px; color: #ff9800;'>Pending: {pendingCount}</span></p>");
                }
                else
                {
                    sb.AppendLine($"<p><strong>{status.Key}:</strong> {status.Value} ??????</p>");
                }
            }

            // ? ????? Overall Statistics Summary
            if (records.Any())
            {
                var allStats = records.Select(r => CalculateCaStatistics(r.record)).Where(s => s.avg > 0).ToList();
                if (allStats.Any())
                {
                    var overallAvg = allStats.Average(s => s.avg);
                    var overallMax = allStats.Max(s => s.max);
                    var overallMin = allStats.Min(s => s.min);

                    sb.AppendLine("<hr style='margin: 15px 0;'>");
                    sb.AppendLine("<h4 style='color: #555; margin-bottom: 10px;'> Overall Thickness Statistics</h4>");
                    sb.AppendLine($"<p style='margin: 5px 0;'><strong>Overall Average:</strong> {overallAvg:F2}</p>");
                    sb.AppendLine($"<p style='margin: 5px 0;'><strong>Highest Value:</strong> {overallMax:F2}</p>");
                    sb.AppendLine($"<p style='margin: 5px 0;'><strong>Lowest Value:</strong> {overallMin:F2}</p>");
                    sb.AppendLine($"<p style='margin: 5px 0;'><strong>Range:</strong> {overallMax - overallMin:F2}</p>");
                }
            }

            sb.AppendLine("</div>");

            // Footer
            sb.AppendLine("<hr style='margin-top: 30px;'>");
            sb.AppendLine("<p style='color: #666; font-size: 12px;'>");
            sb.AppendLine("This is an automated alert from Polishing System.<br>");
            sb.AppendLine($"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}<br>");
            sb.AppendLine("? Rescreen OK = Found matching record in Th100Record table<br>");
            sb.AppendLine("?? Statistics calculated from Ca1-Ca5 In/Out values (sample 10 pcs.)<br>");
            sb.AppendLine("?? Diff = Min - Max value");
            sb.AppendLine("</p>");

            // Added link section
            sb.AppendLine("<div style='text-align: center; margin-top: 15px; padding: 10px; background-color: #e3f2fd; border-radius: 5px;'>");
            sb.AppendLine("<p style='margin: 0; font-size: 14px; font-weight: bold; color: #1976d2;'>");
            sb.AppendLine("?? View Detailed Dashboard:");
            sb.AppendLine("</p>");
            sb.AppendLine("<p style='margin: 5px 0 0 0;'>");
            sb.AppendLine("<a href='http://172.18.106.100:9014/pol-page' style='color: #1976d2; text-decoration: none; font-weight: bold; font-size: 14px; padding: 8px 16px; background-color: #ffffff; border: 2px solid #1976d2; border-radius: 4px; display: inline-block;' target='_blank'>");
            sb.AppendLine("?? Open Polishing Dashboard");
            sb.AppendLine("</a>");
            sb.AppendLine("</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>");


            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        // ✅ เพิ่ม method สำหรับกำหนดชื่อรอบเวลา
        private string GetTimeSlotName(int hour)
        {
            return hour switch
            {
                0 => "Midnight Shift (00:00)", // เพิ่มเที่ยงคืน
                >= 6 and < 12 => "Morning Shift (06:00)",
                12 => "Noon Shift (12:00)", // ระบุเที่ยงชัด
                >= 18 and < 24 => "Evening Shift (18:00)",
                >= 1 and < 6 => "Early Morning Shift",
                >= 13 and < 18 => "Afternoon Shift",
                _ => "Unknown Shift"
            };
        }

        private string GenerateEmptyReportBody(DateTime reportTime, DateTime fromTime, DateTime toTime)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head><meta charset='UTF-8'></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; margin: 20px;'>");

            var timeSlot = GetTimeSlotName(reportTime.Hour);
            sb.AppendLine($"<h2 style='color: #4caf50;'>✅ Polishing Report - All Clear ({timeSlot})</h2>");
            sb.AppendLine($"<p><strong>Report Time:</strong> {reportTime:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Period:</strong> {fromTime:yyyy-MM-dd HH:mm} - {toTime:yyyy-MM-dd HH:mm}</p>");
            sb.AppendLine("<p style='color: #4caf50; font-size: 16px;'><strong>🎉 No issues found in this period</strong></p>");
            sb.AppendLine("<p>All Polishing processes are running normally with no Rescreen, Hold, or Scrap items.</p>");
            sb.AppendLine("<hr><p><small>Automated Report - Polishing System</small></p>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        private string GenerateErrorReportBody(Exception ex, DateTime errorTime)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head><meta charset='UTF-8'></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; margin: 20px;'>");
            sb.AppendLine($"<h2 style='color: #f44336;'>❌ Polishing Report System Error</h2>");
            sb.AppendLine($"<p><strong>Error Time:</strong> {errorTime:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Error Message:</strong> {ex.Message}</p>");
            sb.AppendLine("<p style='color: #f44336;'><strong>Please check the Email Scheduler system</strong></p>");
            sb.AppendLine($"<details><summary>Technical Details</summary><pre>{ex}</pre></details>");
            sb.AppendLine("<hr><p><small>Error Alert - Polishing System</small></p>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }
        
        private async Task SendEmail(string subject, string body, IServiceProvider serviceProvider)
        {
            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port);
            smtpClient.EnableSsl = _smtpSettings.EnableSsl;
            smtpClient.UseDefaultCredentials = _smtpSettings.UseDefaultCredentials;
            smtpClient.Timeout = 30000; // ✅ เพิ่ม timeout 30 วินาที

            if (!_smtpSettings.UseDefaultCredentials)
            {
                smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
            }

            using var mailMessage = new MailMessage();
            var fromAddress = new MailAddress(_smtpSettings.FromEmail,
                string.IsNullOrWhiteSpace(_smtpSettings.FromName) ? "Polishing System" : _smtpSettings.FromName);
            mailMessage.From = fromAddress;

            // Get recipients
            var emailRecipientsService = serviceProvider.GetRequiredService<IEmailRecipientsService>();
            var recipients = emailRecipientsService.GetAllRecipients();

            // Add recipients
            int totalRecipients = 0;
            foreach (var email in recipients.To ?? new List<string>())
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    mailMessage.To.Add(email.Trim());
                    totalRecipients++;
                }
            }

            foreach (var email in recipients.Cc ?? new List<string>())
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    mailMessage.CC.Add(email.Trim());
                    totalRecipients++;
                }
            }

            foreach (var email in recipients.Bcc ?? new List<string>())
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    mailMessage.Bcc.Add(email.Trim());
                    totalRecipients++;
                }
            }

            if (totalRecipients == 0)
            {
                throw new InvalidOperationException("No valid email recipients configured");
            }

            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;
            mailMessage.Priority = MailPriority.Normal;

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("📧 Email sent to {TotalRecipients} recipients", totalRecipients);
        }

        // ✅ เพิ่ม method สำหรับส่งเมลทดสอบ manual
        public async Task SendTestEmailAsync()
        {
            _logger.LogInformation("🧪 Sending manual test email...");
            await SendScheduledEmailWithRetry(GetThailandTime());
        }
        // ✅ เพิ่ม method สำหรับดูสถานะ service
        public ServiceStatus GetServiceStatus()
        {
            var currentTime = GetThailandTime();
            return new ServiceStatus
            {
                IsEnabled = _scheduleSettings.EnableSchedule,
                CurrentTime = currentTime,
                LastSentTime = _lastSentTime == DateTime.MinValue ? null : _lastSentTime,
                NextScheduledTimes = GetNextScheduledTimes(currentTime),
                ScheduleHours = _scheduleSettings.ScheduleHours,
                Configuration = new Dictionary<string, object>
                {
                    ["CheckIntervalSeconds"] = _scheduleSettings.CheckIntervalSeconds,
                    ["DelayAfterSendSeconds"] = _scheduleSettings.DelayAfterSendSeconds,
                    ["DataLookbackHours"] = _scheduleSettings.DataLookbackHours,
                    ["SendEmptyReports"] = _scheduleSettings.SendEmptyReports,
                    ["MaxRetryAttempts"] = _scheduleSettings.MaxRetryAttempts,
                    ["SmtpHost"] = _smtpSettings.Host,
                    ["SmtpPort"] = _smtpSettings.Port,
                    ["LastSentByHour"] = _lastSentByHour.ToDictionary(
                        kvp => kvp.Key.ToString(),
                        kvp => kvp.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                }
            };
        }


        // ✅ เพิ่ม method สำหรับคำนวณเวลาส่งครั้งต่อไป
        private List<DateTime> GetNextScheduledTimes(DateTime currentTime)
        {
            var nextTimes = new List<DateTime>();
            var today = currentTime.Date;

            // หาเวลาที่เหลือในวันนี้
            foreach (var hour in _scheduleSettings.ScheduleHours.OrderBy(h => h))
            {
                var scheduledTime = today.AddHours(hour);
                if (scheduledTime > currentTime)
                {
                    nextTimes.Add(scheduledTime);
                }
            }

            // ถ้าไม่มีเวลาที่เหลือในวันนี้ ให้เอาจากวันพรุ่งนี้
            if (!nextTimes.Any())
            {
                var tomorrow = today.AddDays(1);
                nextTimes.AddRange(_scheduleSettings.ScheduleHours.OrderBy(h => h)
                    .Select(hour => tomorrow.AddHours(hour)));
            }

            return nextTimes.Take(3).ToList();
        }

    }

    // ✅ เพิ่ม class สำหรับ Service Status
    //public class ServiceStatus
    //{
    //    public bool IsEnabled { get; set; }
    //    public DateTime CurrentTime { get; set; }
    //    public DateTime? LastSentTime { get; set; }
    //    public List<DateTime> NextScheduledTimes { get; set; } = new();
    //    public List<int> ScheduleHours { get; set; } = new();
    //    public Dictionary<string, object> Configuration { get; set; } = new();
    //}
    // ✅ ปรับปรุง Extension method สำหรับ register service
    public static class EmailServiceExtensions
    {
        public static IServiceCollection AddEmailScheduleService(this IServiceCollection services)
        {// Register EmailRecipientsService first
            services.AddSingleton<IEmailRecipientsService, EmailRecipientsService>();
            // Register เฉพาะ Hosted Service เท่านั้น
            services.AddHostedService<EmailScheduleService>();
            return services;
        }

        public static IServiceCollection AddEmailScheduleSettings(this IServiceCollection services,
            Action<ScheduleSettings> configureSchedule = null)
        {
            services.Configure<ScheduleSettings>(settings =>
            {
                // Default values - เพิ่มเที่ยงคืน
                settings.EnableSchedule = true;
                settings.ScheduleHours = new List<int> { 0, 6, 12, 18 }; // 00:00, 06:00, 12:00, 18:00
                settings.CheckIntervalSeconds = 60;
                settings.DelayAfterSendSeconds = 90;
                settings.DataLookbackHours = 6;
                settings.SendEmptyReports = true;
                settings.SendErrorNotifications = true;
                settings.MaxRetryAttempts = 3;
                settings.RetryDelaySeconds = 30;

                configureSchedule?.Invoke(settings);
            });


            return services;
        }
        public static IServiceCollection AddEmailScheduleWithManualAccess(this IServiceCollection services)
        {
            // Register EmailRecipientsService
            services.AddSingleton<IEmailRecipientsService, EmailRecipientsService>();

            // Register as singleton first
            services.AddSingleton<EmailScheduleService>();

            // Then register as hosted service using the singleton instance
            services.AddHostedService<EmailScheduleService>(provider =>
                provider.GetRequiredService<EmailScheduleService>());

            return services;
        }

        


    }
}