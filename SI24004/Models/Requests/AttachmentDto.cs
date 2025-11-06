namespace SI24004.Models.Requests
{
    public class AttachmentDto
    {
        public Guid? Id { get; set; }
        public string? AttachmentName { get; set; }
        public string? AttachementPath { get; set; }
        public string? AttachementType { get; set; }
        public DateTime? UploadDate { get; set; }
        public Guid? RequestId { get; set; }
        public byte[]? AttachementFileData { get; set; }
        public bool? IsDeleted { get; set; }
        public string Category { get; set; }
        public string? AttachmentSize { get; set; }

        public string? AttachmentFileLocation { get; set; }
    }
}
