using System.Collections.Generic;

namespace SI24004.Models.DTOs
{
    public class BatchSummary
    {
        public int TotalSearched { get; set; }
        public int Found { get; set; }
        public List<string> NotFound { get; set; } = new();
        public List<BatchError> Errors { get; set; } = new();
    }
}
