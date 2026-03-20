using Microsoft.EntityFrameworkCore;
using SI24004.Models.PostgreSQL;
using SI24004.Models.SqlServer;

namespace SI24004.Services
{
    public class IISKeepAliveService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IISKeepAliveService> _logger;

        public IISKeepAliveService(
            IServiceProvider serviceProvider,
            ILogger<IISKeepAliveService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("?? IISKeepAliveService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

                    // Keep-alive ping ???????? database connection
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var dbContext = scope.ServiceProvider.GetService<ThicknessContext>();
                            if (dbContext != null)
                            {
                                await dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
                                _logger.LogDebug("?? Database keep-alive successful");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug("Keep-alive DB check failed: {Message}", ex.Message);
                        }
                    });

                    _logger.LogInformation("?? Keep-alive ping - {Time}", DateTime.Now.ToString("HH:mm:ss"));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in keep-alive service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("?? IISKeepAliveService stopped");
        }
    }


}
