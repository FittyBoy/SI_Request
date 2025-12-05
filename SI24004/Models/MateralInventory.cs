using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class MateralInventory
{
    public Guid Id { get; set; }

    public string? MatName { get; set; }

    public int? MatQuantity { get; set; }

    public Guid? MatTypeId { get; set; }

    public string? Case { get; set; }

    public DateOnly? ExpDate { get; set; }

    public Guid? EmpId { get; set; }

    public string? Shift { get; set; }

    public string? Product { get; set; }

    public string? LotNumber { get; set; }

    public string? Location { get; set; }

    public DateOnly? InsertDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
