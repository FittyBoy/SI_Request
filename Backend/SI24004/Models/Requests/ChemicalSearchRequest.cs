using System;

namespace SI24004.Models.Requests
{
    public class ChemicalSearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public string SearchType { get; set; } = "all";
        public bool ExactMatch { get; set; } = false;
        public bool CaseSensitive { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}