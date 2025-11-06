using Microsoft.Extensions.Options;
using SI24004.Models.Requests;
using SI24004.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace SI24004.Service
{
    public class EmailScheduleManager : IEmailScheduleManager
    {
        private readonly ILogger<EmailScheduleManager> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ScheduleSettings _scheduleSettings;
        private readonly SmtpSettings _smtpSettings;
        private readonly TimeZoneInfo _thailandTimeZone;

        private Timer? _scheduleTimer;
        private Timer? _keepAliveTimer;
        private DateTime _lastRunTime = DateTime.MinValue;
        private DateTime _lastKeepAlive = DateTime.Now;
        private readonly Dictionary<int, DateTime> _lastSentByHour = new();
        private bool _isRunning = false;
        private int _executionCount = 0;
        private readonly object _lockObject = new object();

        public bool IsRunning => _isRunning;

        public EmailScheduleManager(
            ILogger<EmailScheduleManager> logger,
            IServiceProvider serviceProvider,
            IOptions<ScheduleSettings> scheduleSettings,
            IOptions<SmtpSettings> smtpSettings)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _scheduleSettings = scheduleSettings?.Value ?? throw new ArgumentNullException(nameof(scheduleSettings));
            _smtpSettings = smtpSettings?.Value ?? throw new ArgumentNullException(nameof(smtpSettings));

            // Initialize Thailand timezone with fallback
            _thailandTimeZone = InitializeThailandTimeZone();
        }

        private TimeZoneInfo InitializeThailandTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (Exception)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
                }
                catch (Exception)
                {
                    _logger.LogWarning("Cannot find Thailand timezone, using UTC+7 offset");
                    return TimeZoneInfo.CreateCustomTimeZone("Thailand", TimeSpan.FromHours(7), "Thailand", "Thailand");
                }
            }
        }

        public Task StartAsync()
        {
            _logger.LogInformation("🚀 Starting EmailScheduleManager");

            if (!_scheduleSettings.EnableSchedule)
            {
                _logger.LogWarning("⚠️ Email schedule is disabled in configuration");
                return Task.CompletedTask;
            }

            LogConfiguration();
            StopTimers();

            var checkInterval = TimeSpan.FromSeconds(Math.Max(_scheduleSettings.CheckIntervalSeconds, 30));
            _scheduleTimer = new Timer(CheckScheduleCallback, null, TimeSpan.Zero, checkInterval);
            _keepAliveTimer = new Timer(KeepAliveCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            _logger.LogInformation("✅ EmailScheduleManager started with {Interval}s interval", checkInterval.TotalSeconds);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _logger.LogInformation("🛑 Stopping EmailScheduleManager");
            StopTimers();
            return Task.CompletedTask;
        }

        private void LogConfiguration()
        {
            _logger.LogInformation("Configuration:");
            _logger.LogInformation("- EnableSchedule: {EnableSchedule}", _scheduleSettings.EnableSchedule);
            _logger.LogInformation("- ScheduleHours: [{Hours}]", string.Join(", ", _scheduleSettings.ScheduleHours));
            _logger.LogInformation("- CheckInterval: {Interval}s", _scheduleSettings.CheckIntervalSeconds);
            _logger.LogInformation("- DataLookback: {Hours}h", _scheduleSettings.DataLookbackHours);
            _logger.LogInformation("- SMTP: {Host}:{Port}", _smtpSettings.Host, _smtpSettings.Port);
        }

        private void StopTimers()
        {
            _scheduleTimer?.Dispose();
            _keepAliveTimer?.Dispose();
            _scheduleTimer = null;
            _keepAliveTimer = null;
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

        private async void CheckScheduleCallback(object? state)
        {
            if (!Monitor.TryEnter(_lockObject, TimeSpan.FromSeconds(5)))
            {
                _logger.LogDebug("⏸️ Schedule check already running, skipping");
                return;
            }

            try
            {
                await CheckAndExecuteScheduledTask();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in schedule check callback");
            }
            finally
            {
                Monitor.Exit(_lockObject);
            }
        }

        private void KeepAliveCallback(object? state)
        {
            _lastKeepAlive = DateTime.Now;
            _logger.LogDebug("❤️ Keep-alive ping at {Time}", _lastKeepAlive.ToString("HH:mm:ss"));

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetService<sqlServerContext>();
                    if (dbContext != null)
                    {
                        await dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Keep-alive DB check failed: {Message}", ex.Message);
                }
            });
        }

        private async Task CheckAndExecuteScheduledTask()
        {
            if (_isRunning)
            {
                _logger.LogDebug("📧 Email task already running, skipping");
                return;
            }

            var currentTime = GetThailandTime();
            var currentHour = currentTime.Hour;
            var currentMinute = currentTime.Minute;

            // Log status every 10 minutes
            if (currentMinute % 10 == 0 && currentMinute < 2)
            {
                _logger.LogInformation("🕐 Status Check - Time: {Time}, IsScheduledHour: {IsScheduled}, LastSent: {LastSent}",
                    currentTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    _scheduleSettings.ScheduleHours.Contains(currentHour),
                    _lastRunTime == DateTime.MinValue ? "Never" : _lastRunTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            if (!ShouldSendEmail(currentTime))
            {
                return;
            }

            await ExecuteEmailTaskInternal();
        }

        private bool ShouldSendEmail(DateTime currentTime)
        {
            var currentHour = currentTime.Hour;
            var currentMinute = currentTime.Minute;

            // Must be scheduled hour and at minute 0 (or close to it)
            if (!_scheduleSettings.ScheduleHours.Contains(currentHour) || currentMinute > 2)
                return false;

            // Check if already sent recently for this hour
            if (_lastSentByHour.ContainsKey(currentHour))
            {
                var lastSentForThisHour = _lastSentByHour[currentHour];
                var timeDiff = currentTime - lastSentForThisHour;

                if (timeDiff.TotalMinutes < 50)
                {
                    return false;
                }
            }

            // Check global last run time
            var timeSinceLastRun = currentTime - _lastRunTime;
            if (timeSinceLastRun.TotalMinutes < 50)
            {
                _logger.LogDebug("⏸️ Already ran recently: {Minutes} minutes ago", timeSinceLastRun.TotalMinutes);
                return false;
            }

            return true;
        }

        public async Task ExecuteOnDemandAsync()
        {
            if (_isRunning)
            {
                throw new InvalidOperationException("Email task is already running");
            }

            _logger.LogInformation("🎯 Executing email task on demand");
            await ExecuteEmailTaskInternal();
        }

        private async Task ExecuteEmailTaskInternal()
        {
            _isRunning = true;
            _executionCount++;
            var startTime = GetThailandTime();

            try
            {
                _logger.LogInformation("📧 Starting email task #{Count} at {Time}",
                    _executionCount, startTime.ToString("yyyy-MM-dd HH:mm:ss"));

                await SendScheduledEmailWithRetry(startTime);

                _lastRunTime = startTime;
                _lastSentByHour[startTime.Hour] = startTime;

                var duration = DateTime.Now - startTime;
                _logger.LogInformation("✅ Email task #{Count} completed in {Duration}ms",
                    _executionCount, duration.TotalMilliseconds);

                if (_scheduleSettings.DelayAfterSendSeconds > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_scheduleSettings.DelayAfterSendSeconds));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Email task #{Count} failed", _executionCount);

                if (_scheduleSettings.SendErrorNotifications)
                {
                    await SendErrorNotificationAsync(ex);
                }
                throw;
            }
            finally
            {
                _isRunning = false;
            }
        }

        private async Task<bool> SendScheduledEmailWithRetry(DateTime currentTime)
        {
            for (int attempt = 1; attempt <= _scheduleSettings.MaxRetryAttempts; attempt++)
            {
                try
                {
                    _logger.LogInformation("📧 Sending email attempt {Attempt}/{MaxAttempts}",
                        attempt, _scheduleSettings.MaxRetryAttempts);

                    await SendScheduledEmailAsync(currentTime);
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

        private async Task SendScheduledEmailAsync(DateTime reportTime)
        {
            using var scope = _serviceProvider.CreateScope();
            var sqlContext = scope.ServiceProvider.GetRequiredService<sqlServerContext>();
            var fromTime = reportTime.AddHours(-_scheduleSettings.DataLookbackHours);
            var toTime = reportTime;

            _logger.LogInformation("?? Querying data from {FromTime} to {ToTime}",
                fromTime.ToString("yyyy-MM-dd HH:mm"), toTime.ToString("yyyy-MM-dd HH:mm"));

            // Query database with initial filter - let's first check all recent records
            _logger.LogInformation("?? First, checking recent records without result filter...");

            var allRecentRecords = await sqlContext.ThRecords
                .Where(th => th.DateProcess >= fromTime.Date.AddDays(-1)) // Get records from yesterday onwards
                .OrderByDescending(th => th.DateProcess)
                .Take(10)
                .ToListAsync();

            _logger.LogInformation("?? Found {RecentCount} recent records (sample):", allRecentRecords.Count);
            foreach (var recent in allRecentRecords.Take(5))
            {
                _logger.LogInformation("   ?? DateProcess: {Date}, ImobileType: '{ImobileType}', Result: '{Result}'",
                    recent.DateProcess.ToString("yyyy-MM-dd"), recent.ImobileType ?? "NULL", recent.Status ?? "NULL");
            }

            // Now query with the original filters
            var records = await sqlContext.ThRecords
                .Where(th => th.ImobileType == "Polishing" &&
                    (th.Status.ToLower() == "rescreen" || th.Status.ToLower() == "hold" || th.Status.ToLower() == "scrap"))
                .ToListAsync();

            _logger.LogInformation("?? Found {RecordCount} total records matching filters", records.Count);

            // Debug: Log some sample records to understand the data structure
            if (records.Any())
            {
                // Get latest date from records
                var latestDate = records.Max(r => r.DateProcess.Date);
                var latestRecords = records.Where(r => r.DateProcess.Date == latestDate).Take(10).ToList();

                _logger.LogInformation("?? Records from latest date ({LatestDate}):", latestDate.ToString("yyyy-MM-dd"));
                foreach (var record in latestRecords)
                {
                    var combinedDateTime = record.DateProcess.Date.Add(record.TimeProcess.TimeOfDay);
                    _logger.LogInformation("   ?? DateProcess: {DateProcess}, TimeProcess: {TimeProcess}, Combined: {Combined}, Result: '{Result}'",
                        record.DateProcess.ToString("yyyy-MM-dd HH:mm:ss"),
                        record.TimeProcess.ToString("yyyy-MM-dd HH:mm:ss"),
                        combinedDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        record.Status);
                }

                // Check if we have any records from today
                var todayRecords = records.Where(r => r.DateProcess.Date >= DateTime.Today).ToList();
                _logger.LogInformation("?? Records from today or later: {TodayCount}", todayRecords.Count);
            }
            else
            {
                _logger.LogWarning("?? No records found matching the filter criteria!");
                _logger.LogInformation("   Filter: ImobileType='Polishing' AND Result IN ('rescreen','hold','scrap')");

                // Let's check what ImobileTypes and Results we actually have
                var distinctTypes = await sqlContext.ThRecords
                    .Where(th => th.DateProcess >= fromTime.Date.AddDays(-7)) // Last week
                    .Select(th => new { th.ImobileType, th.Status })
                    .Distinct()
                    .Take(20)
                    .ToListAsync();

                _logger.LogInformation("?? Available ImobileType/Result combinations in last 7 days:");
                foreach (var combo in distinctTypes)
                {
                    _logger.LogInformation("   ?? ImobileType: '{ImobileType}', Result: '{Result}'",
                        combo.ImobileType ?? "NULL", combo.Status ?? "NULL");
                }
            }

            // Filter records by time range with improved logic
            var filteredRecords = new List<ThRecord>();
            int debugCount = 0;

            foreach (var th in records)
            {
                try
                {
                    var recordDateTime = th.DateProcess.Date.Add(th.TimeProcess.TimeOfDay);
                    bool isInRange = recordDateTime >= fromTime && recordDateTime <= toTime;

                    // Debug log for first few records
                    if (debugCount < 5)
                    {
                        _logger.LogInformation("?? Record {Index}: DateTime={DateTime}, FromTime={FromTime}, ToTime={ToTime}, InRange={InRange}",
                            debugCount,
                            recordDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            fromTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            toTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            isInRange);
                        debugCount++;
                    }

                    if (isInRange)
                    {
                        filteredRecords.Add(th);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("?? Error processing record date/time: {Error}", ex.Message);
                }
            }

            // Sort the filtered records
            filteredRecords = filteredRecords
                .OrderByDescending(th => th.DateProcess.Date.Add(th.TimeProcess.TimeOfDay))
                .ToList();

            _logger.LogInformation("?? Filtered to {FilteredCount} records in time range", filteredRecords.Count);

            // If no records after filtering, try alternative approach
            if (!filteredRecords.Any() && records.Any())
            {
                _logger.LogInformation("?? No records found with standard filtering, trying alternative approach...");

                // Alternative approach: Filter by date only first, then by time
                var dateFilteredRecords = records
                    .Where(th => th.DateProcess.Date >= fromTime.Date && th.DateProcess.Date <= toTime.Date)
                    .ToList();

                _logger.LogInformation("?? Found {DateFilteredCount} records matching date range", dateFilteredRecords.Count);

                // Then filter by combined datetime
                filteredRecords = dateFilteredRecords
                    .Where(th =>
                    {
                        var recordDateTime = th.DateProcess.Date.Add(th.TimeProcess.TimeOfDay);
                        return recordDateTime >= fromTime && recordDateTime <= toTime;
                    })
                    .OrderByDescending(th => th.DateProcess.Date.Add(th.TimeProcess.TimeOfDay))
                    .ToList();

                _logger.LogInformation("?? Final filtered count after time filtering: {FinalCount}", filteredRecords.Count);
            }

            // Check Rescreen status
            var enrichedRecords = new List<(ThRecord record, bool isRescreenCompleted)>();
            if (filteredRecords?.Any() == true)
            {
                foreach (var record in filteredRecords)
                {
                    bool isRescreenCompleted = false;
                    if (string.Equals(record.Status, "Rescreen", StringComparison.OrdinalIgnoreCase))
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

            // Generate and send email
            string emailBody, subject;
            var timeSlot = GetTimeSlotName(reportTime.Hour);
            if (enrichedRecords?.Any() == true)
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
            _logger.LogInformation("? Email sent successfully: {Subject}", subject);
        }
        public async Task SendTestEmailAsync()
        {
            _logger.LogInformation("🧪 Sending test email...");
            await SendScheduledEmailWithRetry(GetThailandTime());
        }

        private async Task SendErrorNotificationAsync(Exception ex)
        {
            try
            {
                _logger.LogInformation("📧 Sending error notification");

                using var scope = _serviceProvider.CreateScope();
                var errorBody = GenerateErrorReportBody(ex, GetThailandTime());
                await SendEmail($"Polishing System Error - {GetThailandTime():yyyy-MM-dd HH:mm}",
                              errorBody, scope.ServiceProvider);
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "❌ Failed to send error notification");
            }
        }

        private async Task SendEmail(string subject, string body, IServiceProvider serviceProvider)
        {
            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port);
            smtpClient.EnableSsl = _smtpSettings.EnableSsl;
            smtpClient.UseDefaultCredentials = _smtpSettings.UseDefaultCredentials;
            smtpClient.Timeout = 30000;

            if (!_smtpSettings.UseDefaultCredentials)
            {
                smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
            }

            using var mailMessage = new MailMessage();
            var fromAddress = new MailAddress(_smtpSettings.FromEmail,
                string.IsNullOrWhiteSpace(_smtpSettings.FromName) ? "Polishing System" : _smtpSettings.FromName);
            mailMessage.From = fromAddress;

            // Get recipients from service
            var emailRecipientsService = serviceProvider.GetService<IEmailRecipientsService>();
            if (emailRecipientsService != null)
            {
                var recipients = emailRecipientsService.GetAllRecipients();
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

                _logger.LogInformation("📧 Email will be sent to {TotalRecipients} recipients", totalRecipients);
            }
            else
            {
                // Fallback to default recipient
                mailMessage.To.Add("anupong.ohok@agc.com");
                _logger.LogWarning("⚠️ EmailRecipientsService not available, using default recipient");
            }

            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;
            mailMessage.Priority = MailPriority.Normal;

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("✅ Email sent successfully");
        }

        // Enhanced email body generation methods
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
            var diff =  max - min;

            return (avg, max, min, diff);
        }

        private string GenerateEnhancedEmailBody(IEnumerable<(ThRecord record, bool isRescreenCompleted)> records,
    DateTime reportTime, DateTime fromTime, DateTime toTime)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; margin: 20px;'>");

            // Header
            sb.AppendLine($"<h2 style='color: #d32f2f;'>Polishing Alert Report</h2>");
            sb.AppendLine($"<p><strong>Report Time:</strong> {reportTime:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Period:</strong> {fromTime:yyyy-MM-dd HH:mm} - {toTime:yyyy-MM-dd HH:mm}</p>");
            sb.AppendLine($"<p><strong>Total Issues:</strong> {records.Count()}</p>");

            // Table
            sb.AppendLine("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%; margin-top: 20px; font-size: 12px;'>");

            // Table Header
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr style='background-color: #f5f5f5;'>");
            sb.AppendLine("<th>No.</th>");
            sb.AppendLine("<th>Status</th>");
            sb.AppendLine("<th>PO Lot</th>");
            sb.AppendLine("<th>MC</th>");
            sb.AppendLine("<th style='background-color: #e3f2fd;'>Avg</th>");
            sb.AppendLine("<th style='background-color: #ffebee;'>Max</th>");
            sb.AppendLine("<th style='background-color: #e8f5e8;'>Min</th>");
            sb.AppendLine("<th style='background-color: #fff3e0;'>Diff</th>");
            sb.AppendLine("<th>Rescreen Status</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");

            // Table Body
            sb.AppendLine("<tbody>");
            int rowNumber = 1;

            foreach (var item in records)
            {
                var record = item.record;
                var isRescreenCompleted = item.isRescreenCompleted;
                var (avg, max, min, diff) = CalculateCaStatistics(record);

                string poLot = $"{record.LotPo ?? ""}{record.McPo ?? ""}{record.NoPo ?? ""}";
                string mc = record.McPo ?? "";
                string status = (record.Status ?? "").ToUpper();

                string rescreenStatus = "";
                string statusIcon = "";

                // Set status icon and rescreen status
                switch (status)
                {
                    case "RESCREEN":
                        statusIcon = "";
                        if (isRescreenCompleted)
                        {
                            rescreenStatus = "<span style='color: #4caf50; font-weight: bold;'>Completed</span>";
                        }
                        else
                        {
                            rescreenStatus = "<span style='color: #ff9800; font-weight: bold;'>Pending</span>";
                        }
                        break;
                    case "HOLD":
                        statusIcon = "";
                        rescreenStatus = "<span style='color: #2196f3; font-weight: bold;'>On Hold</span>";
                        break;
                    case "SCRAP":
                        statusIcon = "";
                        rescreenStatus = "<span style='color: #f44336; font-weight: bold;'>Scrapped</span>";
                        break;
                    default:
                        statusIcon = "";
                        rescreenStatus = "<span style='color: #757575;'>-</span>";
                        break;
                }

                string bgColor = status switch
                {
                    "RESCREEN" when isRescreenCompleted => "#e8f5e8",
                    "RESCREEN" => "#ffebee",
                    "HOLD" => "#e3f2fd",
                    "SCRAP" => "#ffebee",
                    _ => "#ffffff"
                };

                sb.AppendLine($"<tr style='background-color: {bgColor};'>");
                sb.AppendLine($"<td style='text-align: center;'>{rowNumber}</td>");
                sb.AppendLine($"<td>{statusIcon} {status}</td>");
                sb.AppendLine($"<td>{poLot}</td>");
                sb.AppendLine($"<td>{mc}</td>");
                sb.AppendLine($"<td style='text-align: center;'>{(avg > 0 ? avg.ToString("F3") : "-")}</td>");
                sb.AppendLine($"<td style='text-align: center;'>{(max > 0 ? max.ToString("F3") : "-")}</td>");
                sb.AppendLine($"<td style='text-align: center;'>{(min > 0 ? min.ToString("F3") : "-")}</td>");
                sb.AppendLine($"<td style='text-align: center;'>{(avg > 0 ? (diff * 1000).ToString("F0") : "-")}</td>");
                sb.AppendLine($"<td>{rescreenStatus}</td>");
                sb.AppendLine("</tr>");

                rowNumber++;
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Summary
            sb.AppendLine("<div style='margin-top: 20px; padding: 15px; background-color: #f0f0f0; border-radius: 5px;'>");
            sb.AppendLine("<h3>Summary</h3>");

            var statusCounts = records.GroupBy(r => (r.record.Status ?? "Unknown").ToUpper())
                                     .ToDictionary(g => g.Key, g => g.Count());

            // Define status display order and styling
            var statusConfig = new Dictionary<string, (string displayName, string color, string icon)>
                {
                    { "RESCREEN", ("Rescreen", "#ff9800", "") },
                    { "HOLD", ("Hold", "#2196f3", "") },
                    { "SCRAP", ("Scrap", "#f44336", "") }
                };


            foreach (var statusInfo in statusConfig)
            {
                var statusKey = statusInfo.Key;
                var (displayName, color, icon) = statusInfo.Value;

                if (statusCounts.ContainsKey(statusKey))
                {
                    var count = statusCounts[statusKey];

                    if (statusKey == "RESCREEN")
                    {
                        var rescreenRecords = records.Where(r => (r.record.Status ?? "").ToUpper() == "RESCREEN").ToList();
                        var completedCount = rescreenRecords.Count(r => r.isRescreenCompleted);
                        var pendingCount = rescreenRecords.Count() - completedCount;

                        sb.AppendLine($"<p style='margin: 8px 0; padding: 10px; background-color: #fff3e0; border-left: 4px solid {color}; border-radius: 4px;'>");
                        sb.AppendLine($"<strong style='color: {color};'>{icon} {displayName}:</strong> <span style='font-size: 16px; font-weight: bold;'>{count}</span> items");
                        sb.AppendLine($"<br><span style='margin-left: 20px; color: #4caf50;'>Completed: <strong>{completedCount}</strong></span>");
                        sb.AppendLine($"<span style='margin-left: 15px; color: #ff9800;'>Pending: <strong>{pendingCount}</strong></span>");
                        sb.AppendLine("</p>");
                    }
                    else
                    {
                        sb.AppendLine($"<p style='margin: 8px 0; padding: 10px; background-color: #f8f9fa; border-left: 4px solid {color}; border-radius: 4px;'>");
                        sb.AppendLine($"<strong style='color: {color};'>{icon} {displayName}:</strong> <span style='font-size: 16px; font-weight: bold;'>{count}</span> items");
                        sb.AppendLine("</p>");
                    }
                }
                else
                {
                    // Show 0 count for statuses that don't have any records
                    sb.AppendLine($"<p style='margin: 8px 0; padding: 10px; background-color: #f8f9fa; border-left: 4px solid #e0e0e0; border-radius: 4px;'>");
                    sb.AppendLine($"<strong style='color: #757575;'>{icon} {displayName}:</strong> <span style='font-size: 16px; font-weight: bold; color: #757575;'>0</span> items");
                    sb.AppendLine("</p>");
                }
            }
            // Add total summary
            var totalItems = statusCounts.Values.Sum();
            sb.AppendLine($"<div style='margin-top: 15px; padding: 12px; background-color: #e3f2fd; border: 2px solid #2196f3; border-radius: 6px; text-align: center;'>");
            sb.AppendLine($"<strong style='color: #1976d2; font-size: 18px;'>Total Issues: {totalItems}</strong>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>");


            // Footer
            sb.AppendLine("<hr style='margin-top: 30px;'>");
            sb.AppendLine("<p style='color: #666; font-size: 12px;'>");
            sb.AppendLine("Automated report from Polishing System<br>");
            sb.AppendLine($"Generated at: {GetThailandTime():yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("</p>");
            // Added link section
            sb.AppendLine("<div style='text-align: center; margin-top: 15px; padding: 10px; background-color: #e3f2fd; border-radius: 5px;'>");
            sb.AppendLine("<p style='margin: 0; font-size: 14px; font-weight: bold; color: #1976d2;'>");
            sb.AppendLine("View Detailed Dashboard:");
            sb.AppendLine("</p>");
            sb.AppendLine("<p style='margin: 5px 0 0 0;'>");
            sb.AppendLine("<a href='http://172.18.106.100:9014/pol-page' style='color: #1976d2; text-decoration: none; font-weight: bold; font-size: 14px; padding: 8px 16px; background-color: #ffffff; border: 2px solid #1976d2; border-radius: 4px; display: inline-block;' target='_blank'>");
            sb.AppendLine("Open Polishing Dashboard");
            sb.AppendLine("</a>");
            sb.AppendLine("</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private string GenerateEmptyReportBody(DateTime reportTime, DateTime fromTime, DateTime toTime)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; margin: 20px;'>");

            var timeSlot = GetTimeSlotName(reportTime.Hour);
            sb.AppendLine($"<h2 style='color: #4caf50;'>✅ Polishing Report - All Clear ({timeSlot})</h2>");
            sb.AppendLine($"<p><strong>Report Time:</strong> {reportTime:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Period:</strong> {fromTime:yyyy-MM-dd HH:mm} - {toTime:yyyy-MM-dd HH:mm}</p>");
            sb.AppendLine("<p style='color: #4caf50; font-size: 16px;'><strong>🎉 No issues found in this period</strong></p>");
            sb.AppendLine("<p>All Polishing processes are running normally with no Rescreen, Hold, or Scrap items.</p>");
            sb.AppendLine("<hr><p><small>📧 Automated Report - Polishing System</small></p>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        private string GenerateErrorReportBody(Exception ex, DateTime errorTime)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; margin: 20px;'>");
            sb.AppendLine($"<h2 style='color: #f44336;'>❌ Polishing System Error</h2>");
            sb.AppendLine($"<p><strong>Error Time:</strong> {errorTime:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Error Message:</strong> {ex.Message}</p>");
            sb.AppendLine("<p style='color: #f44336;'><strong>⚠️ Please check the Email Scheduler system</strong></p>");
            sb.AppendLine($"<details><summary>Technical Details</summary><pre>{ex}</pre></details>");
            sb.AppendLine("<hr><p><small>📧 Error Alert - Polishing System</small></p>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        private string GetTimeSlotName(int hour)
        {
            return hour switch
            {
                0 => "Midnight Shift (00:00)",
                >= 6 and < 12 => "Morning Shift (06:00)",
                12 => "Noon Shift (12:00)",
                >= 18 and < 24 => "Evening Shift (18:00)",
                >= 1 and < 6 => "Early Morning Shift",
                >= 13 and < 18 => "Afternoon Shift",
                _ => "Unknown Shift"
            };
        }

        public object GetStatus()
        {
            var currentTime = GetThailandTime();
            return new
            {
                IsRunning = _isRunning,
                IsEnabled = _scheduleSettings.EnableSchedule,
                CurrentTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss"),
                LastRunTime = _lastRunTime == DateTime.MinValue ? "Never" : _lastRunTime.ToString("yyyy-MM-dd HH:mm:ss"),
                LastKeepAlive = _lastKeepAlive.ToString("yyyy-MM-dd HH:mm:ss"),
                ExecutionCount = _executionCount,
                ScheduleHours = _scheduleSettings.ScheduleHours,
                CheckIntervalSeconds = _scheduleSettings.CheckIntervalSeconds,
                NextScheduledHour = GetNextScheduledHour(currentTime),
                MinutesUntilNext = GetMinutesUntilNextSchedule(currentTime),
                NextScheduledTimes = GetNextScheduledTimes(currentTime),
                LastSentByHour = _lastSentByHour.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value.ToString("yyyy-MM-dd HH:mm:ss")),
                Configuration = new
                {
                    DataLookbackHours = _scheduleSettings.DataLookbackHours,
                    DelayAfterSendSeconds = _scheduleSettings.DelayAfterSendSeconds,
                    SendEmptyReports = _scheduleSettings.SendEmptyReports,
                    SendErrorNotifications = _scheduleSettings.SendErrorNotifications,
                    MaxRetryAttempts = _scheduleSettings.MaxRetryAttempts,
                    RetryDelaySeconds = _scheduleSettings.RetryDelaySeconds,
                    SmtpHost = _smtpSettings.Host,
                    SmtpPort = _smtpSettings.Port,
                    TimeZone = _thailandTimeZone.DisplayName
                }
            };
        }

        private int GetNextScheduledHour(DateTime currentTime)
        {
            var currentHour = currentTime.Hour;
            return _scheduleSettings.ScheduleHours
                .Where(h => h > currentHour)
                .DefaultIfEmpty(_scheduleSettings.ScheduleHours.Min())
                .First();
        }

        private double GetMinutesUntilNextSchedule(DateTime currentTime)
        {
            var nextHour = GetNextScheduledHour(currentTime);
            var nextSchedule = nextHour > currentTime.Hour
                ? currentTime.Date.AddHours(nextHour)
                : currentTime.Date.AddDays(1).AddHours(nextHour);

            return (nextSchedule - currentTime).TotalMinutes;
        }

        private List<DateTime> GetNextScheduledTimes(DateTime currentTime)
        {
            var nextTimes = new List<DateTime>();
            var today = currentTime.Date;

            // Find remaining times today
            foreach (var hour in _scheduleSettings.ScheduleHours.OrderBy(h => h))
            {
                var scheduledTime = today.AddHours(hour);
                if (scheduledTime > currentTime)
                {
                    nextTimes.Add(scheduledTime);
                }
            }

            // If no more times today, get tomorrow's schedule
            if (!nextTimes.Any())
            {
                var tomorrow = today.AddDays(1);
                nextTimes.AddRange(_scheduleSettings.ScheduleHours.OrderBy(h => h)
                    .Select(hour => tomorrow.AddHours(hour)));
            }

            return nextTimes.Take(3).ToList();
        }

        // Public method for manual testing
        public async Task<ServiceStatus> GetServiceStatusAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var currentTime = GetThailandTime();

            // Get recent record count for status
            int recentRecordCount = 0;
            try
            {
                var sqlContext = scope.ServiceProvider.GetService<sqlServerContext>();
                if (sqlContext != null)
                {
                    var fromTime = currentTime.AddHours(-_scheduleSettings.DataLookbackHours);
                    recentRecordCount = await sqlContext.ThRecords
                        .Where(th => th.ImobileType == "Polishing" &&
                            (th.Status == "Rescreen" || th.Status == "Hold" || th.Status == "Scrap" || th.Status == "RESCREEN"))
                        .CountAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting record count for status");
            }

            return new ServiceStatus
            {
                IsEnabled = _scheduleSettings.EnableSchedule,
                IsRunning = _isRunning,
                CurrentTime = currentTime,
                LastSentTime = _lastRunTime == DateTime.MinValue ? null : _lastRunTime,
                NextScheduledTimes = GetNextScheduledTimes(currentTime),
                ScheduleHours = _scheduleSettings.ScheduleHours,
                ExecutionCount = _executionCount,
                RecentRecordCount = recentRecordCount,
                Configuration = new Dictionary<string, object>
                {
                    ["CheckIntervalSeconds"] = _scheduleSettings.CheckIntervalSeconds,
                    ["DelayAfterSendSeconds"] = _scheduleSettings.DelayAfterSendSeconds,
                    ["DataLookbackHours"] = _scheduleSettings.DataLookbackHours,
                    ["SendEmptyReports"] = _scheduleSettings.SendEmptyReports,
                    ["MaxRetryAttempts"] = _scheduleSettings.MaxRetryAttempts,
                    ["SmtpHost"] = _smtpSettings.Host,
                    ["SmtpPort"] = _smtpSettings.Port,
                    ["TimeZone"] = _thailandTimeZone.DisplayName,
                    ["LastSentByHour"] = _lastSentByHour.ToDictionary(
                        kvp => kvp.Key.ToString(),
                        kvp => kvp.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                }
            };
        }

        // Helper method to force immediate execution (for testing)
        public async Task ForceExecuteAsync()
        {
            if (_isRunning)
            {
                throw new InvalidOperationException("Email task is already running");
            }

            _logger.LogInformation("🚀 Force executing email task");
            await ExecuteEmailTaskInternal();
        }

        // Method to update schedule hours dynamically
        public void UpdateScheduleHours(List<int> newHours)
        {
            if (newHours?.Any() == true && newHours.All(h => h >= 0 && h <= 23))
            {
                _scheduleSettings.ScheduleHours = newHours;
                _logger.LogInformation("📅 Schedule hours updated to: [{Hours}]", string.Join(", ", newHours));
            }
            else
            {
                throw new ArgumentException("Invalid schedule hours. Must be between 0-23.");
            }
        }

        // Method to enable/disable scheduling
        public void SetScheduleEnabled(bool enabled)
        {
            _scheduleSettings.EnableSchedule = enabled;
            _logger.LogInformation("⚙️ Schedule enabled set to: {Enabled}", enabled);

            if (!enabled)
            {
                StopTimers();
                _logger.LogInformation("🛑 Timers stopped due to schedule disable");
            }
            else if (_scheduleTimer == null)
            {
                // Restart if not running
                _ = StartAsync();
            }
        }
    }

    // Enhanced ServiceStatus class
    public class ServiceStatus
    {
        public bool IsEnabled { get; set; }
        public bool IsRunning { get; set; }
        public DateTime CurrentTime { get; set; }
        public DateTime? LastSentTime { get; set; }
        public List<DateTime> NextScheduledTimes { get; set; } = new();
        public List<int> ScheduleHours { get; set; } = new();
        public int ExecutionCount { get; set; }
        public int RecentRecordCount { get; set; }
        public Dictionary<string, object> Configuration { get; set; } = new();

        public string Status => IsEnabled ? (IsRunning ? "Running" : "Waiting") : "Disabled";
        public double MinutesUntilNext => NextScheduledTimes.Any()
            ? (NextScheduledTimes.First() - CurrentTime).TotalMinutes
            : -1;
    }
}