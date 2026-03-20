using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// บันทึกแต่ละ session การรันเปรียบเทียบ AR/UVIR
/// </summary>
public partial class CompareRun
{
    public Guid Id { get; set; }

    public string RunType { get; set; } = null!;

    public DateTime? RunAt { get; set; }

    public DateOnly? RunDate { get; set; }

    public string? RunWeekday { get; set; }

    public int TotalChecked { get; set; }

    public int TotalMatch { get; set; }

    public int TotalMismatch { get; set; }

    public int TotalError { get; set; }

    public int TotalDiffs { get; set; }

    public int MachineNameMismatches { get; set; }

    public bool HasIssues { get; set; }

    public string? ReportFile { get; set; }

    public bool? EmailSent { get; set; }

    public string? EmailTo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CompareResult> CompareResults { get; set; } = new List<CompareResult>();

    public virtual ICollection<MachineNameMismatch> MachineNameMismatchesNavigation { get; set; } = new List<MachineNameMismatch>();
}
