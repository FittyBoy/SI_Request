namespace SI24004.Models.Requests
{
    public class ScheduleSettings
    {
        public bool EnableSchedule { get; set; } = true;
        public List<int> ScheduleHours { get; set; } = new List<int> { 0, 6, 12, 18 };
        public int CheckIntervalSeconds { get; set; } = 30;
        public int DelayAfterSendSeconds { get; set; } = 65;
        public int DataLookbackHours { get; set; } = 6;
        public bool SendEmptyReports { get; set; } = true;
        public bool SendErrorNotifications { get; set; } = true;
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 30;
    }
}
