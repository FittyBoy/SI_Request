namespace SI24004.Models.Requests
{
    public class LotInaDto
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public string LotNo { get; set; }
        public bool isDeleted { get; set; }
    }
}
