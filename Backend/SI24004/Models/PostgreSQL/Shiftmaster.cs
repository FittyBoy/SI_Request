using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Shiftmaster
{
    public Guid Id { get; set; }

    public string? Shiftcode { get; set; }

    public string? Shiftname { get; set; }

    public TimeOnly? Starttime { get; set; }

    public TimeOnly? Endtime { get; set; }

    public bool? Isactive { get; set; }

    public DateOnly? Createddate { get; set; }

    public DateOnly? Updateddate { get; set; }

    public virtual ICollection<Employeemaster> Employeemasters { get; set; } = new List<Employeemaster>();

    public virtual ICollection<Materalinventory> Materalinventories { get; set; } = new List<Materalinventory>();

    public virtual ICollection<Materialreceiverecord> Materialreceiverecords { get; set; } = new List<Materialreceiverecord>();
}
