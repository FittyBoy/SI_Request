namespace SI24004.Models.DTOs
{
    public class SvhcSubstanceData
    {
        public System.Collections.Generic.List<QaSubstance> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

}
