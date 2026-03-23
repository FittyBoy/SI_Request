using SI24004.Services.Interfaces;
using Microsoft.Extensions.Options;
using SI24004.Models.DTOs;
using SI24004.Models.PostgreSQL;
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Text;
using SI24004.Models.SqlServer;

namespace SI24004.Services
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
            _logger.LogInformation("?? Starting EmailScheduleManager");

            if (!_scheduleSettings.EnableSchedule)
            {
                _logger.LogWarning("?? Email schedule is disabled in configuration");
                return Task.CompletedTask;
            }

            LogConfiguration();
            StopTimers();

            var checkInterval = TimeSpan.FromSeconds(Math.Max(_scheduleSettings.CheckIntervalSeconds, 30));
            _scheduleTimer = new Timer(CheckScheduleCallback, null, TimeSpan.Zero, checkInterval);
            _keepAliveTimer = new Timer(KeepAliveCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            _logger.LogInformation("? EmailScheduleManager started with {Interval}s interval", checkInterval.TotalSeconds);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _logger.LogInformation("?? Stopping EmailScheduleManager");
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
                _logger.LogDebug("?? Schedule check already running, skipping");
                return;
            }

            try
            {
                await CheckAndExecuteScheduledTask();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error in schedule check callback");
            }
            finally
            {
                Monitor.Exit(_lockObject);
            }
        }

        private void KeepAliveCallback(object? state)
        {
            _lastKeepAlive = DateTime.Now;
            _logger.LogDebug("?? Keep-alive ping at {Time}", _lastKeepAlive.ToString("HH:mm:ss"));

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetService<ThicknessContext>();
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
                _logger.LogDebug("?? Email task already running, skipping");
                return;
            }

            var currentTime = GetThailandTime();
            var currentHour = currentTime.Hour;
            var currentMinute = currentTime.Minute;

            // Log status every 10 minutes
            if (currentMinute % 10 == 0 && currentMinute < 2)
            {
                _logger.LogInformation("?? Status Check - Time: {Time}, IsScheduledHour: {IsScheduled}, LastSent: {LastSent}",
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
                _logger.LogDebug("?? Already ran recently: {Minutes} minutes ago", timeSinceLastRun.TotalMinutes);
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

            _logger.LogInformation("?? Executing email task on demand");
            await ExecuteEmailTaskInternal();
        }

        private async Task ExecuteEmailTaskInternal()
        {
            _isRunning = true;
            _executionCount++;
            var startTime = GetThailandTime();

            try
            {
                _logger.LogInformation("?? Starting email task #{Count} at {Time}",
                    _executionCount, startTime.ToString("yyyy-MM-dd HH:mm:ss"));

                await SendScheduledEmailWithRetry(startTime);

                _lastRunTime = startTime;
                _lastSentByHour[startTime.Hour] = startTime;

                var duration = DateTime.Now - startTime;
                _logger.LogInformation("? Email task #{Count} completed in {Duration}ms",
                    _executionCount, duration.TotalMilliseconds);

                if (_scheduleSettings.DelayAfterSendSeconds > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_scheduleSettings.DelayAfterSendSeconds));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Email task #{Count} failed", _executionCount);

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
                    _logger.LogInformation("?? Sending email attempt {Attempt}/{MaxAttempts}",
                        attempt, _scheduleSettings.MaxRetryAttempts);

                    await SendScheduledEmailAsync(currentTime);
                    _logger.LogInformation("? Email sent successfully on attempt {Attempt}", attempt);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "?? Email sending failed on attempt {Attempt}/{MaxAttempts}: {Error}",
                        attempt, _scheduleSettings.MaxRetryAttempts, ex.Message);

                    if (attempt < _scheduleSettings.MaxRetryAttempts)
                    {
                        _logger.LogInformation("? Waiting {Delay}s before retry...", _scheduleSettings.RetryDelaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(_scheduleSettings.RetryDelaySeconds));
                    }
                }
            }

            return false;
        }

        private async Task SendScheduledEmailAsync(DateTime reportTime)
        {
            using var scope = _serviceProvider.CreateScope();
            var sqlContext = scope.ServiceProvider.GetRequiredService<ThicknessContext>();
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
                subject = $"[OE-Polishing] แจ้งเตือน LOT ผิดปกติ {timeSlot} — {reportTime:dd/MM/yyyy HH:mm} ({enrichedRecords.Count} รายการ)";
            }
            else if (_scheduleSettings.SendEmptyReports)
            {
                emailBody = GenerateEmptyReportBody(reportTime, fromTime, toTime);
                subject = $"[OE-Polishing] รายงานสถานะ {timeSlot} — {reportTime:dd/MM/yyyy HH:mm} (ปกติ ไม่มีรายการผิดปกติ)";
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
            _logger.LogInformation("?? Sending test email...");
            await SendScheduledEmailWithRetry(GetThailandTime());
        }

        private async Task SendErrorNotificationAsync(Exception ex)
        {
            try
            {
                _logger.LogInformation("?? Sending error notification");

                using var scope = _serviceProvider.CreateScope();
                var errorBody = GenerateErrorReportBody(ex, GetThailandTime());
                await SendEmail($"Polishing System Error - {GetThailandTime():yyyy-MM-dd HH:mm}",
                              errorBody, scope.ServiceProvider);
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "? Failed to send error notification");
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

                _logger.LogInformation("?? Email will be sent to {TotalRecipients} recipients", totalRecipients);
            }
            else
            {
                // Fallback to default recipient
                mailMessage.To.Add("anupong.ohok@agc.com");
                _logger.LogWarning("?? EmailRecipientsService not available, using default recipient");
            }

            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;
            mailMessage.Priority = MailPriority.Normal;

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("? Email sent successfully");
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
            var recordList = records.ToList();
            int total          = recordList.Count;
            int rescreenPending= recordList.Count(r => r.record.Status?.ToUpper() == "RESCREEN" && !r.isRescreenCompleted);
            int rescreenOk     = recordList.Count(r => r.record.Status?.ToUpper() == "RESCREEN" &&  r.isRescreenCompleted);
            int hold           = recordList.Count(r => r.record.Status?.ToUpper() == "HOLD");
            int scrap          = recordList.Count(r => r.record.Status?.ToUpper() == "SCRAP");

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'>");
            sb.AppendLine("<style>");
            sb.AppendLine("  body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 20px; background: #f5f5f5; color: #333; }");
            sb.AppendLine("  .container { max-width: 760px; margin: 0 auto; background: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }");
            sb.AppendLine("  .header { background: linear-gradient(135deg, #c62828, #e53935); padding: 24px 32px; color: #fff; }");
            sb.AppendLine("  .header h1 { margin: 0 0 4px; font-size: 22px; font-weight: 700; }");
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
            sb.AppendLine($"<div class='header'><h1>🔔 แจ้งเตือน LOT ผิดปกติ — OE Polishing</h1>");
            sb.AppendLine($"<p>ช่วงเวลา: {fromTime:dd/MM/yyyy HH:mm} &ndash; {reportTime:dd/MM/yyyy HH:mm} &nbsp;|&nbsp; {GetTimeSlotName(reportTime.Hour)}</p></div>");

            // Summary stats
            sb.AppendLine("<div class='summary'>");
            sb.AppendLine($"  <div class='stat stat-total'><div class='stat-num'>{total}</div><div class='stat-lbl'>Total</div></div>");
            sb.AppendLine($"  <div class='stat stat-rescreen'><div class='stat-num'>{rescreenPending}</div><div class='stat-lbl'>Rescreen</div></div>");
            sb.AppendLine($"  <div class='stat stat-ok'><div class='stat-num'>{rescreenOk}</div><div class='stat-lbl'>Rescreen OK</div></div>");
            sb.AppendLine($"  <div class='stat stat-hold'><div class='stat-num'>{hold}</div><div class='stat-lbl'>Hold</div></div>");
            sb.AppendLine($"  <div class='stat stat-scrap'><div class='stat-num'>{scrap}</div><div class='stat-lbl'>Scrap</div></div>");
            sb.AppendLine("</div>");

            // Table
            sb.AppendLine("<div class='content'><table>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("  <th class='center' style='width:44px'>No.</th>");
            sb.AppendLine("  <th>PO Lot</th>");
            sb.AppendLine("  <th class='center' style='width:60px'>MC</th>");
            sb.AppendLine("  <th class='center'>Status Thickness</th>");
            sb.AppendLine("  <th class='center'>Status</th>");
            sb.AppendLine("</tr></thead><tbody>");

            int rowNumber = 1;
            foreach (var (record, isRescreenCompleted) in recordList)
            {
                string poLot  = $"{record.LotPo ?? ""}{record.McPo ?? ""}{record.NoPo ?? ""}";
                string mc     = record.McPo ?? "";
                string status = (record.Status ?? "").ToUpper();

                string bgColor = status switch
                {
                    "RESCREEN" when isRescreenCompleted => "#f1f8e9",
                    "RESCREEN"                          => "#fff8f8",
                    "HOLD"                              => "#f3f8ff",
                    "SCRAP"                             => "#fff0f3",
                    _                                   => "#ffffff"
                };

                string badgeClass = status switch
                {
                    "RESCREEN" => "badge-rescreen",
                    "HOLD"     => "badge-hold",
                    "SCRAP"    => "badge-scrap",
                    _          => ""
                };

                string finalStatus = (status == "RESCREEN" && isRescreenCompleted)
                    ? "<span class='badge badge-ok'>Rescreen OK</span>" : "";

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
            sb.AppendLine($"<div class='footer'>สร้างอัตโนมัติเมื่อ {DateTime.Now:dd/MM/yyyy HH:mm:ss} &bull; ระบบแจ้งเตือน OE-Polishing &bull; กรุณาอย่า Reply อีเมลฉบับนี้</div>");
            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        private string GenerateEmptyReportBody(DateTime reportTime, DateTime fromTime, DateTime toTime)
        {
            var sb = new StringBuilder();
            var timeSlot = GetTimeSlotName(reportTime.Hour);
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'>");
            sb.AppendLine("<style>");
            sb.AppendLine("  body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 20px; background: #f5f5f5; color: #333; }");
            sb.AppendLine("  .container { max-width: 600px; margin: 0 auto; background: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }");
            sb.AppendLine("  .header { background: linear-gradient(135deg, #2e7d32, #43a047); padding: 24px 32px; color: #fff; }");
            sb.AppendLine("  .header h1 { margin: 0 0 4px; font-size: 20px; font-weight: 700; }");
            sb.AppendLine("  .header p { margin: 0; font-size: 13px; opacity: 0.85; }");
            sb.AppendLine("  .body { padding: 28px 32px; }");
            sb.AppendLine("  .ok-icon { font-size: 48px; text-align: center; margin-bottom: 12px; }");
            sb.AppendLine("  .ok-msg { font-size: 18px; font-weight: 700; color: #2e7d32; text-align: center; margin-bottom: 8px; }");
            sb.AppendLine("  .period { background: #f1f8e9; border-left: 4px solid #66bb6a; padding: 12px 16px; border-radius: 4px; font-size: 13px; margin-top: 20px; }");
            sb.AppendLine("  .footer { padding: 14px 32px; background: #f9f9f9; border-top: 1px solid #eee; font-size: 11px; color: #999; }");
            sb.AppendLine("</style></head>");
            sb.AppendLine("<body><div class='container'>");
            sb.AppendLine($"<div class='header'><h1>✅ รายงานสถานะ OE-Polishing — {timeSlot}</h1>");
            sb.AppendLine($"<p>{reportTime:dd/MM/yyyy HH:mm}</p></div>");
            sb.AppendLine("<div class='body'>");
            sb.AppendLine("<div class='ok-icon'>✅</div>");
            sb.AppendLine("<div class='ok-msg'>ปกติ — ไม่พบ LOT ผิดปกติในช่วงเวลานี้</div>");
            sb.AppendLine("<p style='text-align:center;color:#666;font-size:13px'>กระบวนการ Polishing ทำงานปกติ ไม่มีรายการ Rescreen / Hold / Scrap</p>");
            sb.AppendLine($"<div class='period'><strong>ช่วงเวลาที่ตรวจสอบ:</strong><br>{fromTime:dd/MM/yyyy HH:mm} — {toTime:dd/MM/yyyy HH:mm}</div>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<div class='footer'>สร้างอัตโนมัติเมื่อ {DateTime.Now:dd/MM/yyyy HH:mm:ss} &bull; ระบบแจ้งเตือน OE-Polishing &bull; กรุณาอย่า Reply อีเมลฉบับนี้</div>");
            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        private string GenerateErrorReportBody(Exception ex, DateTime errorTime)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; margin: 20px;'>");
            sb.AppendLine($"<h2 style='color: #f44336;'>? Polishing System Error</h2>");
            sb.AppendLine($"<p><strong>Error Time:</strong> {errorTime:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Error Message:</strong> {ex.Message}</p>");
            sb.AppendLine("<p style='color: #f44336;'><strong>?? Please check the Email Scheduler system</strong></p>");
            sb.AppendLine($"<details><summary>Technical Details</summary><pre>{ex}</pre></details>");
            sb.AppendLine("<hr><p><small>?? Error Alert - Polishing System</small></p>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        private string GetTimeSlotName(int hour)
        {
            return hour switch
            {
                0  => "กะดึก (00:00)",
                >= 1 and < 6  => "กะดึก (ช่วงเช้ามืด)",
                >= 6 and < 12 => "กะเช้า (06:00)",
                12 => "กะบ่าย (12:00)",
                >= 13 and < 18 => "กะบ่าย (ช่วงบ่าย)",
                >= 18 and < 24 => "กะเย็น (18:00)",
                _ => "ไม่ระบุกะ"
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
                var sqlContext = scope.ServiceProvider.GetService<ThicknessContext>();
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

            _logger.LogInformation("?? Force executing email task");
            await ExecuteEmailTaskInternal();
        }

        // Method to update schedule hours dynamically
        public void UpdateScheduleHours(List<int> newHours)
        {
            if (newHours?.Any() == true && newHours.All(h => h >= 0 && h <= 23))
            {
                _scheduleSettings.ScheduleHours = newHours;
                _logger.LogInformation("?? Schedule hours updated to: [{Hours}]", string.Join(", ", newHours));
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
            _logger.LogInformation("?? Schedule enabled set to: {Enabled}", enabled);

            if (!enabled)
            {
                StopTimers();
                _logger.LogInformation("?? Timers stopped due to schedule disable");
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

