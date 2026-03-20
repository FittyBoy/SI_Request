using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// รายละเอียด parameter ที่ต่างกัน
/// </summary>
public partial class ParameterDifference
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int ResultId { get; set; }

    public DateTime RunAt { get; set; }

    public string? Machine { get; set; }

    public string? ModelType { get; set; }

    public string? DatFile { get; set; }

    public string? Parameter { get; set; }

    /// <summary>
    /// ค่าใน QA (master)
    /// </summary>
    public string? MasterValue { get; set; }

    /// <summary>
    /// ค่าใน Production
    /// </summary>
    public string? CompareValue { get; set; }

    public int? MasterLine { get; set; }

    public int? CompareLine { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ComparisonResult Result { get; set; } = null!;

    public virtual RunSession Session { get; set; } = null!;
}
