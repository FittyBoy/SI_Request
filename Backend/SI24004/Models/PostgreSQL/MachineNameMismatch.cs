using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class MachineNameMismatch
{
    public Guid Id { get; set; }

    public Guid RunId { get; set; }

    public int? RowNumber { get; set; }

    public string? MachineName { get; set; }

    public string? ProductName { get; set; }

    public string? MasterUrl { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual CompareRun Run { get; set; } = null!;
}
