using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class VDailySummary
{
    public string? RunType { get; set; }

    public DateOnly? RunDate { get; set; }

    public long? TotalRuns { get; set; }

    public long? TotalChecked { get; set; }

    public long? TotalMatch { get; set; }

    public long? TotalMismatch { get; set; }

    public long? TotalError { get; set; }

    public long? TotalDiffs { get; set; }

    public long? MachineNameMismatches { get; set; }

    public bool? HasIssues { get; set; }
}
