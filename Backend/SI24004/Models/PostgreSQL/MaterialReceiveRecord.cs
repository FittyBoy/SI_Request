using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Materialreceiverecord
{
    public Guid Id { get; set; }

    public string Matname { get; set; } = null!;

    public int Matquantity { get; set; }

    public Guid? Mattypeid { get; set; }

    public string? Case { get; set; }

    public DateOnly? Expdate { get; set; }

    public Guid Empid { get; set; }

    public Guid Shift { get; set; }

    public Guid? Product { get; set; }

    public Guid? Supplier { get; set; }

    public string Lotnumber { get; set; } = null!;

    public string Location { get; set; } = null!;

    public string? Status { get; set; }

    public DateOnly? Insertdate { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual Employeemaster Emp { get; set; } = null!;

    public virtual Materialtype1? Mattype { get; set; }

    public virtual Productmaster? ProductNavigation { get; set; }

    public virtual Shiftmaster ShiftNavigation { get; set; } = null!;

    public virtual Suppliermaster? SupplierNavigation { get; set; }
}
