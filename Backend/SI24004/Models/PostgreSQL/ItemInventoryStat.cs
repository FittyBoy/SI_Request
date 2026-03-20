using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class ItemInventoryStat
{
    public Guid Id { get; set; }

    public string ItemCode { get; set; } = null!;

    public DateOnly PeriodDate { get; set; }

    public string PeriodType { get; set; } = null!;

    public decimal? OpeningBalance { get; set; }

    public decimal? ClosingBalance { get; set; }

    public decimal? MinQuantity { get; set; }

    public DateTime? MinDatetime { get; set; }

    public decimal? MaxQuantity { get; set; }

    public DateTime? MaxDatetime { get; set; }

    public decimal? AvgQuantity { get; set; }

    public decimal? TotalIn { get; set; }

    public decimal? TotalOut { get; set; }

    public decimal? TotalHold { get; set; }

    public decimal? TotalRelease { get; set; }

    public int? CountIn { get; set; }

    public int? CountOut { get; set; }

    public int? CountTransactions { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
