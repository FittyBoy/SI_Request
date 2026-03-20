using SI24004.Models;
using System;
using System.Collections.Generic;

namespace SI24004.Models.Requests
{
    public class ChemicalSearchResponse
    {
        public List<QaSubstance> Svhc { get; set; } = new();
        public List<RegularSubstand> Regular { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SearchType { get; set; } = string.Empty;
        public string DetectedType { get; set; } = string.Empty;
        public double SearchTime { get; set; }
        public bool Success { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
