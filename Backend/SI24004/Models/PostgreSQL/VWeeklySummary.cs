using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class VWeeklySummary
{
    public DateTime? WeekStart { get; set; }

    public long? TotalRuns { get; set; }

    public long? OkCount { get; set; }

    public long? NgCount { get; set; }

    public long? ErrorCount { get; set; }

    public long? TotalNgParams { get; set; }

    public long? MachinesChecked { get; set; }
}
