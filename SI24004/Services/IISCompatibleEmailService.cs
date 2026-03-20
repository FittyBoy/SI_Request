namespace SI24004.Services
{
    public class IISCompatibleEmailService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IISCompatibleEmailService> _logger;

        public IISCompatibleEmailService(
            IServiceProvider serviceProvider,
            ILogger<IISCompatibleEmailService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("?? IISCompatibleEmailService Starting...");

            // ????? EmailScheduleManager
            try
            {
                var emailManager = _serviceProvider.GetService<IEmailScheduleManager>();
                if (emailManager != null)
                {
                    await emailManager.StartAsync();
                    _logger.LogInformation("? EmailScheduleManager started via IISCompatibleEmailService");
                }
                else
                {
                    _logger.LogWarning("?? EmailScheduleManager not found in service provider");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error starting EmailScheduleManager");
            }

            await base.StartAsync(cancellationToken);
            _logger.LogInformation("? IISCompatibleEmailService Started");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("?? IISCompatibleEmailService Stopping...");

            // ???? EmailScheduleManager
            try
            {
                var emailManager = _serviceProvider.GetService<IEmailScheduleManager>();
                if (emailManager != null)
                {
                    await emailManager.StopAsync();
                    _logger.LogInformation("? EmailScheduleManager stopped via IISCompatibleEmailService");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error stopping EmailScheduleManager");
            }

            await base.StopAsync(cancellationToken);
            _logger.LogInformation("? IISCompatibleEmailService Stopped");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("? IISCompatibleEmailService is running");

            // Keep service alive ??? monitor EmailScheduleManager
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // ?? 5 ????
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

                    // Log heartbeat ??????????????? EmailManager
                    var emailManager = _serviceProvider.GetService<IEmailScheduleManager>();
                    if (emailManager != null)
                    {
                        var status = emailManager.GetStatus();
                        _logger.LogInformation("?? IISCompatibleEmailService heartbeat - {Time}, EmailManager: {Status}",
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            emailManager.IsRunning ? "Running" : "Idle");
                    }
                    else
                    {
                        _logger.LogWarning("?? EmailScheduleManager not available in heartbeat check");
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("?? IISCompatibleEmailService cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "?? Error in IISCompatibleEmailService heartbeat");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("?? IISCompatibleEmailService ExecuteAsync ended");
        }
    }


}
