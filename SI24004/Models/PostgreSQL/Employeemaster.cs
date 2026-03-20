using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Employeemaster
{
    public Guid Id { get; set; }

    public string Employeeid { get; set; }

    public string Employeename { get; set; }

    public string Department { get; set; }

    public string Position { get; set; }

    public Guid? Shift { get; set; }

    public bool? Isactive { get; set; }

    public DateOnly? Createddate { get; set; }

    public DateOnly? Updateddate { get; set; }

    public virtual ICollection<Materalinventory> Materalinventories { get; set; } = new List<Materalinventory>();

    public virtual ICollection<Materialreceiverecord> Materialreceiverecords { get; set; } = new List<Materialreceiverecord>();

    public virtual Shiftmaster ShiftNavigation { get; set; }
}
