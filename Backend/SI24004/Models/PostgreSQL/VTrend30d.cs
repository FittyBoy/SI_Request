using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class VTrend30d
{
    public string? RunType { get; set; }

    public DateOnly? RunDate { get; set; }

    public long? Checked { get; set; }

    public long? Matched { get; set; }

    public long? Mismatched { get; set; }

    public long? Errored { get; set; }

    public long? Diffs { get; set; }

    public decimal? MatchRatePct { get; set; }
}
