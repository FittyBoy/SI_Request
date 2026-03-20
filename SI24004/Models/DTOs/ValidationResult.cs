using System.Collections.Generic;

namespace SI24004.Models.DTOs
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}