using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Request
{
    public Guid RequestId { get; set; }

    public string? RequestCode { get; set; }

    public string RequestOptcode { get; set; } = null!;

    public string RequestLicensecode { get; set; } = null!;

    public string? RequestReason { get; set; }

    public string? RequestStatus { get; set; }

    public DateOnly? RequestDate { get; set; }

    public string? RequestType { get; set; }

    public string? RequestRemark { get; set; }

    public string? Requester { get; set; }

    public string? RequestFactory { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<LicenseRecord> LicenseRecords { get; set; } = new List<LicenseRecord>();
}
