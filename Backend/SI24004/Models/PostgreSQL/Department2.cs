using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Department2
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<License> Licenses { get; set; } = new List<License>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
}
