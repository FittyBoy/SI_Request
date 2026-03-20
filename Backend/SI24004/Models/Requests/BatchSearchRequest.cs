using System.Collections.Generic;

namespace SI24004.Models.Requests
{
    public class BatchSearchRequest
    {
        public List<BatchItem> Items { get; set; } = new();
        public bool SkipInvalid { get; set; } = true;
        public bool ExactMatch { get; set; } = false;
        public bool CaseSensitive { get; set; } = false;
    }
}
