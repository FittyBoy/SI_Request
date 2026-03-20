using SI24004.Models.DTOs;
using SI24004.Models.PostgreSQL;
using System.Collections.Generic;

namespace SI24004.Models.DTOs
{
    public class BatchSearchResult
    {
        public string SearchItem { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public List<QaSubstance> QaSubstances { get; set; } = new();
        public List<RegularSubstand> RegularSubstands { get; set; } = new();
        public bool Found { get; set; }
        public string? Error { get; set; }
    }
}