using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Factory
{
    public Guid Id { get; set; }

    public string FacCode { get; set; } = null!;

    public string FacName { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
