using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// Departments within each division, e.g. PO (Polishing) in Front-end
/// </summary>
public partial class Department1
{
    public int Id { get; set; }

    public int DivisionId { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public int? SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ControlPlanEntry1> ControlPlanEntry1s { get; set; } = new List<ControlPlanEntry1>();

    public virtual Division1 Division { get; set; } = null!;
}
