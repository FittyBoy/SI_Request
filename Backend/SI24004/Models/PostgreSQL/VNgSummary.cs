using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class VNgSummary
{
    public DateTime? RunAt { get; set; }

    public string? Machine { get; set; }

    public string? ModelType { get; set; }

    public string? Status { get; set; }

    public string? MasterFolder { get; set; }

    public string? CompareFolder { get; set; }

    public int? MissingInMaster { get; set; }

    public int? ContentNgFiles { get; set; }

    public int? ContentNgParams { get; set; }

    public string? Message { get; set; }

    public string? JsonFile { get; set; }

    public string? TxtFile { get; set; }
}
