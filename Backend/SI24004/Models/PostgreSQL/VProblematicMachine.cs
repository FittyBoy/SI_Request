using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class VProblematicMachine
{
    public string? RunType { get; set; }

    public string? MachineName { get; set; }

    public string? ProductName { get; set; }

    public long? TotalComparisons { get; set; }

    public long? Matches { get; set; }

    public long? Mismatches { get; set; }

    public long? Errors { get; set; }

    public long? TotalDiffs { get; set; }

    public decimal? MatchRatePct { get; set; }

    public DateTime? LastSeen { get; set; }
}
