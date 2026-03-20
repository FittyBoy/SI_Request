using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Productmaster
{
    public Guid Id { get; set; }

    public string Productcode { get; set; } = null!;

    public string Productname { get; set; } = null!;

    public string? Description { get; set; }

    public bool? Isactive { get; set; }

    public DateOnly? Createddate { get; set; }

    public DateOnly? Updateddate { get; set; }

    public virtual ICollection<Materalinventory> Materalinventories { get; set; } = new List<Materalinventory>();

    public virtual ICollection<Materialreceiverecord> Materialreceiverecords { get; set; } = new List<Materialreceiverecord>();
}
