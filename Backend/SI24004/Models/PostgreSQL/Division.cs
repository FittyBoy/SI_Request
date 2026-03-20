using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// Production divisions: Front-end, Middle-end, Back-end, Support
/// </summary>
public partial class Division
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public string? Color { get; set; }

    public int? SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}
