using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// รายละเอียดความแตกต่างในแต่ละ cell/sheet
/// </summary>
public partial class CompareDifference
{
    public Guid Id { get; set; }

    public Guid ResultId { get; set; }

    public string DiffType { get; set; } = null!;

    public string? SheetName { get; set; }

    public string? CellRef { get; set; }

    public string? MasterValue { get; set; }

    public string? CompareValue { get; set; }

    public string? Dimension { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual CompareResult Result { get; set; } = null!;
}
