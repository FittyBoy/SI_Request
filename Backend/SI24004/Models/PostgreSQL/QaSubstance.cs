using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class QaSubstance
{
    public Guid Id { get; set; }

    public string? SubstanceName { get; set; }

    public string? CasNo { get; set; }

    public string? EcNo { get; set; }

    public string? ReasonForInclusion { get; set; }

    public string? Uses { get; set; }

    public DateOnly? SvhcCandidate { get; set; }
}
