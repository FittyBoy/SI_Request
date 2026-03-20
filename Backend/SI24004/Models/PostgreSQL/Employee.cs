using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Employee
{
    public Guid OptId { get; set; }

    public string OptCode { get; set; } = null!;

    public string OptName { get; set; } = null!;

    public string OptSurname { get; set; } = null!;

    public Guid? OptFacId { get; set; }

    public string? OptDep { get; set; }

    public string? OptFac { get; set; }

    public string? OptShift { get; set; }

    public string? OptPosition { get; set; }

    public DateOnly? OptStartdate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? OptDepId { get; set; }

    public string? OptStatus { get; set; }

    public DateOnly? OptEnddate { get; set; }

    public string? OptEmpType { get; set; }

    public virtual Section? OptDepNavigation { get; set; }

    public virtual Factory? OptFacNavigation { get; set; }
}
