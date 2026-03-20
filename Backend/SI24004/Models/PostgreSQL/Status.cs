using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Status
{
    public Guid Id { get; set; }

    public string? StatusName { get; set; }

    // Legacy properties - not mapped to DB columns, kept for controller compatibility
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int? Ordinal { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Guid? StatusTypeId { get; set; }
}
