using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class RunSession1
{
    public int Id { get; set; }

    public DateTime RunAt { get; set; }

    public string? ExcelFile { get; set; }

    public int? TotalChecked { get; set; }

    public int? TotalOk { get; set; }

    public int? TotalNg { get; set; }

    public int? TotalError { get; set; }

    public string? JsonFile { get; set; }

    public string? TxtFile { get; set; }

    public bool? EmailSent { get; set; }

    public bool? WeeklyEmailSent { get; set; }

    public virtual ICollection<ComparisonResult1> ComparisonResult1s { get; set; } = new List<ComparisonResult1>();

    public virtual ICollection<ParameterDifference1> ParameterDifference1s { get; set; } = new List<ParameterDifference1>();
}
