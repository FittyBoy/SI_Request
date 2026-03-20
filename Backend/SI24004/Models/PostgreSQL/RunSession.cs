using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// แต่ละครั้งที่รัน compare.py
/// </summary>
public partial class RunSession
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

    /// <summary>
    /// ส่ง weekly summary แล้วหรือยัง (วันจันทร์)
    /// </summary>
    public bool? WeeklyEmailSent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ComparisonResult> ComparisonResults { get; set; } = new List<ComparisonResult>();

    public virtual ICollection<ParameterDifference> ParameterDifferences { get; set; } = new List<ParameterDifference>();
}
