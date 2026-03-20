using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// Admin users for the centralized data system
/// </summary>
public partial class Admin1
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ControlPlanEntry1> ControlPlanEntry1CreatedByNavigations { get; set; } = new List<ControlPlanEntry1>();

    public virtual ICollection<ControlPlanEntry1> ControlPlanEntry1UpdatedByNavigations { get; set; } = new List<ControlPlanEntry1>();
}
