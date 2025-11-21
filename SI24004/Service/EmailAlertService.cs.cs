using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using SI24004.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Collections.Generic;
using SI24004.ModelsSQL;

public class EmailAlertService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeZoneInfo _thailandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public EmailAlertService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    private DateTime? MergeDateAndTime(DateTime? datePart, DateTime? timePart)
    {
        if (!datePart.HasValue || !timePart.HasValue)
            return null;

        return new DateTime(
            datePart.Value.Year,
            datePart.Value.Month,
            datePart.Value.Day,
            timePart.Value.Hour,
            timePart.Value.Minute,
            timePart.Value.Second
        );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ThicknessContext>();

                    var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _thailandTimeZone);
                    var since = now.AddMinutes(-5);

                    var allRecords = await db.ThRecords
                        .Where(r =>
                            (r.Status == "Hold" || r.Status == "Scrap" || r.Status == "Rescreen") &&
                            r.DateProcess != null &&
                            r.TimeProcess != null)
                        .ToListAsync();

                    var recentCriticalRecords = allRecords
                        .Select(r =>
                        {
                            var merged = MergeDateAndTime(r.DateProcess, r.TimeProcess);
                            return new { Record = r, MergedDateTime = merged };
                        })
                        .Where(x => x.MergedDateTime != null && x.MergedDateTime >= since)
                        .Select(x => x.Record)
                        .ToList();

                    if (recentCriticalRecords.Any())
                    {
                        await SendEmailAlertAsync(recentCriticalRecords);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 EmailAlertService Error: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }


    private async Task SendEmailAlertAsync(List<ThRecord> records)
    {
        string subject = $"🚨 แจ้งเตือนสถานะผิดปกติใน TH_RECORD จำนวน {records.Count} รายการ";
        string body = "<h3>พบรายการที่มีสถานะ Hold / Scrap / Rescreen</h3><ul>";

        foreach (var r in records)
        {
            body += $"<li>LotId: {r.LotId}, Status: {r.Status}, Date: {r.DateProcess:yyyy-MM-dd HH:mm}</li>";
        }

        body += "</ul>";

        var mail = new MailMessage();
        mail.From = new MailAddress("PO_ThicknessAlert@email.com");
        mail.To.Add("anupong.ohok@agc.com");
        mail.To.Add("recipient2@email.com");
        mail.Subject = subject;
        mail.Body = body;
        mail.IsBodyHtml = true;

        using var smtp = new SmtpClient("django.core.mail.backends.smtp.EmailBackend")
        {
            Port = 587,
            Credentials = new System.Net.NetworkCredential("agcmgw.agc.jp", "yourpassword"),
            EnableSsl = true,
        };

        await smtp.SendMailAsync(mail);
        Console.WriteLine("📧 Email alert sent.");
    }
}
