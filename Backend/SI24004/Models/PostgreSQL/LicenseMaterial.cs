using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class LicenseMaterial
{
    public Guid Id { get; set; }

    public Guid? LicenseId { get; set; }

    public string? MaterialCode { get; set; }

    public string? MaterialName { get; set; }

    public string? MaterialType { get; set; }

    public int? Rev { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual License? License { get; set; }
}
