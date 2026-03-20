using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class IssueStatus
{
    public Guid Id { get; set; }

    public string IssueId { get; set; } = null!;

    public string CurrentStatus { get; set; } = null!;

    public string? StatusHistory { get; set; }

    public DateTime? IssuedDate { get; set; }

    public DateTime? HoldDate { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public string? SectionId { get; set; }

    public string? UserId { get; set; }

    public int? TotalItems { get; set; }

    public int? ReceivedItems { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }
}
