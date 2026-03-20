namespace SI24004.Models.DTOs
{
    public class AddStockRequest
    {
        public int Quantity { get; set; }
        public string? LotNumber { get; set; }
        public string? Location { get; set; }

    }
}
