using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Shift
{
    public Guid Id { get; set; }

    public string? ShiftName { get; set; }
}
