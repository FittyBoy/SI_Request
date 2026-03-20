using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class LicenseSo
{
    public Guid Id { get; set; }

    public Guid? LicenseId { get; set; }

    public string? SosCode { get; set; }

    public int? Rev { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual License? License { get; set; }
}
