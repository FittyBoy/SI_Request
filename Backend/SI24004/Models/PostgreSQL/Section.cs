using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Section
{
    public Guid Id { get; set; }

    public Guid? DepartmentId { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Legacy aliases for controller compatibility
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? SectionCode => Code;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? SectionName => Name;

    public virtual Department2? Department { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<License> Licenses { get; set; } = new List<License>();

    public virtual ICollection<Process> Processes { get; set; } = new List<Process>();

    public virtual ICollection<User1> User1s { get; set; } = new List<User1>();
}
