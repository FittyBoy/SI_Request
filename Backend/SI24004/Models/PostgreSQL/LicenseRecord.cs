using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class LicenseRecord
{
    public Guid RecordId { get; set; }

    public Guid? RequestId { get; set; }

    public string RecordLicensecode { get; set; } = null!;

    public string RecordOptcode { get; set; } = null!;

    public DateOnly? RecordIssuetem { get; set; }

    public DateOnly? RecordIssuecer { get; set; }

    public DateOnly? RecordExpiredcer { get; set; }

    public DateOnly? RecordVerdate { get; set; }

    public int? RecordVercount { get; set; }

    public string? RecordStatus { get; set; }

    public string? RecordLevel { get; set; }

    public decimal? ExamScore { get; set; }

    public decimal? ExamTotal { get; set; }

    public decimal? ExamPercent { get; set; }

    public string? Answers { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Request? Request { get; set; }
}
