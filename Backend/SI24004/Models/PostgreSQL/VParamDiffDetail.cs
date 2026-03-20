using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class VParamDiffDetail
{
    public DateTime? RunAt { get; set; }

    public string? Machine { get; set; }

    public string? ModelType { get; set; }

    public string? DatFile { get; set; }

    public string? Parameter { get; set; }

    public string? QaValue { get; set; }

    public string? ProdValue { get; set; }

    public int? QaLine { get; set; }

    public int? ProdLine { get; set; }

    public int? SessionId { get; set; }

    public int? ResultId { get; set; }
}
