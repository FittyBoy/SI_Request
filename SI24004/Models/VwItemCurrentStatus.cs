using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class VwItemCurrentStatus
{
    public string? ItemCode { get; set; }

    public string? ItemName { get; set; }

    public decimal? CurrentBalance { get; set; }

    public decimal? TotalHold { get; set; }

    public long? HoldCount { get; set; }

    public decimal? AvailableQuantity { get; set; }

    public DateTime? LastTransactionDate { get; set; }
}
