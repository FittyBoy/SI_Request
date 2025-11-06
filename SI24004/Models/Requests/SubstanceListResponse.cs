using System.Collections.Generic;

namespace SI24004.Models.Responses
{
    public class SubstanceListResponse
    {
        public bool Success { get; set; }
        public List<RegularSubstand> Data { get; set; }
        public PaginationInfo Pagination { get; set; }
        public System.DateTime Timestamp { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }
}
