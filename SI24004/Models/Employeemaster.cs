using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class Employeemaster
{
    public Guid Id { get; set; }

    public string Employeeid { get; set; }

    public string Employeename { get; set; }

    public string Department { get; set; }

    public string Position { get; set; }

    public string Shift { get; set; }

    public bool? Isactive { get; set; }

    public DateOnly? Createddate { get; set; }

    public DateOnly? Updateddate { get; set; }
}
