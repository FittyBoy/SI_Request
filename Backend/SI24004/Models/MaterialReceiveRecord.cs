using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class Materialreceiverecord
{
    public Guid Id { get; set; }

    public string Matname { get; set; }

    public int Matquantity { get; set; }

    public Guid? Mattypeid { get; set; }

    public string Case { get; set; }

    public DateOnly? Expdate { get; set; }

    public Guid Empid { get; set; }

    public Guid Shift { get; set; }

    public Guid? Product { get; set; }

    public Guid? Supplier { get; set; }

    public string Lotnumber { get; set; }

    public string Location { get; set; }

    public string Status { get; set; }

    public DateOnly? Insertdate { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual Employeemaster Emp { get; set; }

    public virtual Materialtype1 Mattype { get; set; }

    public virtual Productmaster ProductNavigation { get; set; }

    public virtual Shiftmaster ShiftNavigation { get; set; }

    public virtual Suppliermaster SupplierNavigation { get; set; }
}
