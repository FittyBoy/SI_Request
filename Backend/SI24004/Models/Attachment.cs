using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class Attachment
{
    public Guid Id { get; set; }

    public string? AttachmentName { get; set; }

    public string? AttachementPath { get; set; }

    public string? AttachementType { get; set; }

    public DateTime? UploadDate { get; set; }

    public byte[] AttachementFileData { get; set; }

    public bool? IsDeleted { get; set; }

    public Guid? RequestId { get; set; }

    public string? Category { get; set; }

    public string? AttachmentSize { get; set; }

    public string? AttachmentFileLocation { get; set; }

    public virtual ICollection<AviRequest> AviRequests { get; set; } = new List<AviRequest>();

    public virtual ICollection<DwRequest> DwRequests { get; set; } = new List<DwRequest>();

    public virtual ICollection<InaRequest> InaRequests { get; set; } = new List<InaRequest>();
}
