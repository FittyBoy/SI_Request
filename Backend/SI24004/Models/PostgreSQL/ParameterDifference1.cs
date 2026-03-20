using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class ParameterDifference1
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int ResultId { get; set; }

    public DateTime RunAt { get; set; }

    public string? Machine { get; set; }

    public string? ModelType { get; set; }

    public string? DatFile { get; set; }

    public string? Parameter { get; set; }

    public string? MasterValue { get; set; }

    public string? CompareValue { get; set; }

    public int? MasterLine { get; set; }

    public int? CompareLine { get; set; }

    public virtual ComparisonResult1 Result { get; set; } = null!;

    public virtual RunSession1 Session { get; set; } = null!;
}
