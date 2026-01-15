using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class Suppliermaster
{
    public Guid Id { get; set; }

    public string Suppliercode { get; set; }

    public string Suppliername { get; set; }

    public string Contactperson { get; set; }

    public string Phone { get; set; }

    public string Email { get; set; }

    public string Address { get; set; }

    public bool? Isactive { get; set; }

    public DateOnly? Createddate { get; set; }

    public DateOnly? Updateddate { get; set; }

    public virtual ICollection<Materalinventory> Materalinventories { get; set; } = new List<Materalinventory>();

    public virtual ICollection<Materialreceiverecord> Materialreceiverecords { get; set; } = new List<Materialreceiverecord>();
}
