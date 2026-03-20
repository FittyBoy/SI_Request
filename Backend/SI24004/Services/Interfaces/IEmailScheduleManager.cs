namespace SI24004.Services
{
    public interface IEmailScheduleManager
    {
        Task StartAsync();
        Task StopAsync();
        Task ExecuteOnDemandAsync();
        Task SendTestEmailAsync();
        object GetStatus();
        bool IsRunning { get; }
        Task<ServiceStatus> GetServiceStatusAsync(); // ????? method ???
        Task ForceExecuteAsync(); // ????? method ???
        void UpdateScheduleHours(List<int> newHours); // ????? method ???
        void SetScheduleEnabled(bool enabled); // ????? method ???
    }


}
