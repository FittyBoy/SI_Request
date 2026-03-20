namespace SI24004.Models.DTOs
{
    public class SearchOptions
    {
        public bool ExactMatch { get; set; }
        public bool CaseSensitive { get; set; }
        public int MaxResults { get; set; } = 10000;
    }
}
