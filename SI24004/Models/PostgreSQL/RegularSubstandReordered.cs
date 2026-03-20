using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class RegularSubstandReordered
{
    public Guid? Id { get; set; }

    public string? SubstanceChemical { get; set; }

    public string? SubstanceIdentifier { get; set; }

    public string? SubstanceCasNo { get; set; }

    public string? SubstanceThresholdLimit { get; set; }

    public string? SubstanceScope { get; set; }

    public string? SubstanceExamples { get; set; }

    public string? SubstanceReferences { get; set; }
}
