using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class AviRequest
{
    public Guid Id { get; set; }

    public string RequestName { get; set; }

    public Guid UserId { get; set; }

    public string RequestDescription { get; set; }

    public Guid? AttachmentId { get; set; }

    public DateTime? RequestDate { get; set; }

    public bool RequestApprove { get; set; }

    public DateTime? ApproveDate { get; set; }

    public bool Active { get; set; }

    public bool? IsDeleted { get; set; }

    public string RequestCode { get; set; }

    public virtual Attachment Attachment { get; set; }

    public virtual User User { get; set; }
}
