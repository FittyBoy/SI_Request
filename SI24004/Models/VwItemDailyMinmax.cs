using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class VwItemDailyMinmax
{
    public string ItemCode { get; set; }

    public DateOnly? TransactionDate { get; set; }

    public decimal? MinQuantity { get; set; }

    public decimal? MaxQuantity { get; set; }

    public decimal? AvgQuantity { get; set; }

    public long? TransactionCount { get; set; }

    public DateTime? FirstTransaction { get; set; }

    public DateTime? LastTransaction { get; set; }

    public decimal? OpeningBalance { get; set; }

    public decimal? ClosingBalance { get; set; }
}
