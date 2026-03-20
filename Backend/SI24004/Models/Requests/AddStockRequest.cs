namespace SI24004.Models.Requests
{
    public class AddStockRequest
    {
        public int Quantity { get; set; }
        public string? LotNumber { get; set; }
        public string? Location { get; set; }

    }
}
