namespace SI24004.Models.Requests
{
    public class MaterialReceiveUpdateRequest
    {
        public string MatName { get; set; } = string.Empty;
        public int MatQuantity { get; set; }
        public string? MatTypeId { get; set; }
        public string? Case { get; set; }
        public DateOnly? ExpDate { get; set; }
        public string EmpId { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public string LotNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

}
