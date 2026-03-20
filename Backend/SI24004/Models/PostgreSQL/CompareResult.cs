using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// ผลการเปรียบเทียบแต่ละคู่ไฟล์
/// </summary>
public partial class CompareResult
{
    public Guid Id { get; set; }

    public Guid RunId { get; set; }

    public int SeqNo { get; set; }

    public string? MachineName { get; set; }

    public string? ProductName { get; set; }

    public string? MasterUrl { get; set; }

    public string? CompareUrl { get; set; }

    public string? MasterFilename { get; set; }

    public string? CompareFilename { get; set; }

    public string Status { get; set; } = null!;

    public int DiffCount { get; set; }

    public bool? MachineNameOk { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CompareDifference> CompareDifferences { get; set; } = new List<CompareDifference>();

    public virtual CompareRun Run { get; set; } = null!;
}
