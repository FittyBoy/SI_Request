namespace SI24004.Models.Requests
{
    public class SvhcSubstanceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public SvhcSubstanceData Data { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
