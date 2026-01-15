using System.ComponentModel.DataAnnotations;

namespace SI24004.Models.Requests
{
    public class MaterialReceiveUpdateRequest
    {
        [Required]
        public string MatName { get; set; } = string.Empty;

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int MatQuantity { get; set; }

        public string? MatTypeId { get; set; } // Guid as string

        public string? Case { get; set; }

        public DateOnly? ExpDate { get; set; }

        [Required]
        public string EmpId { get; set; } = string.Empty; // Guid as string

        [Required]
        public string Shift { get; set; } = string.Empty; // Guid as string

        public string? Product { get; set; } // Guid as string

        public string? Supplier { get; set; } // Guid as string

        [Required]
        public string LotNumber { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty; // Location code (string)
    }
}