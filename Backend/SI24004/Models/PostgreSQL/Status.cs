using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Status
{
    public Guid Id { get; set; }

    public string? StatusName { get; set; }
}
