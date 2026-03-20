using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class DwRequest
{
    public Guid Id { get; set; }

    public string? DrawingCode { get; set; }

    public string? RequestCode { get; set; }

    public string? DrawingName { get; set; }

    public Guid SectionId { get; set; }

    public Guid DrawingTypeId { get; set; }

    public Guid StatusId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? UpdateBy { get; set; }

    public string? DrawingDescription { get; set; }

    public Guid? UserId { get; set; }

    public Guid? AttachmentId { get; set; }

    public bool Active { get; set; }

    public bool IsDelete { get; set; }

    public bool? DrawingRevise { get; set; }

    public virtual Attachment Attachment { get; set; }

    public virtual Drawing DrawingType { get; set; }

    public virtual Section Section { get; set; }

    public virtual Status Status { get; set; }

    public virtual User User { get; set; }
}
