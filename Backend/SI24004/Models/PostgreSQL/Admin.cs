using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// Admin users for the centralized data system
/// </summary>
public partial class Admin
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ControlPlanEntry> ControlPlanEntryCreatedByNavigations { get; set; } = new List<ControlPlanEntry>();

    public virtual ICollection<ControlPlanEntry> ControlPlanEntryUpdatedByNavigations { get; set; } = new List<ControlPlanEntry>();
}
