namespace SI24004.Models.Requests
{
    public class EmailRecipients
    {
        public List<string> AlertEmails { get; set; } = new List<string>();
        public List<string> ErrorEmails { get; set; } = new List<string>();
        public List<string> ReportEmails { get; set; } = new List<string>();
        public List<string> To { get; set; } = new();
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
    }
}
