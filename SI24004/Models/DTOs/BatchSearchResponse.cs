using System;
using System.Collections.Generic;

namespace SI24004.Models.DTOs
{
    public class BatchSearchResponse
    {
        public List<BatchSearchResult> Results { get; set; } = new();
        public BatchSummary Summary { get; set; } = new();
        public double SearchTime { get; set; }
        public bool Success { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
