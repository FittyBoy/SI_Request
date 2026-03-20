using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// ผลเปรียบเทียบแต่ละคู่ folder
/// </summary>
public partial class ComparisonResult
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public DateTime RunAt { get; set; }

    public int? No { get; set; }

    public string? Machine { get; set; }

    public string? ModelType { get; set; }

    public string? MasterFolder { get; set; }

    public string? CompareFolder { get; set; }

    public string? Status { get; set; }

    public string? Message { get; set; }

    public int? MasterFileCount { get; set; }

    public int? CompareFileCount { get; set; }

    public int? CommonFileCount { get; set; }

    public int? MissingInMaster { get; set; }

    public int? MissingInCompare { get; set; }

    public int? ContentNgFiles { get; set; }

    public int? ContentNgParams { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ParameterDifference> ParameterDifferences { get; set; } = new List<ParameterDifference>();

    public virtual RunSession Session { get; set; } = null!;
}
