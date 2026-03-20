using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class ItemHoldDetail
{
    public Guid Id { get; set; }

    public string IssueId { get; set; }

    public string ItemCode { get; set; }

    public string ItemName { get; set; }

    public string LotNumber { get; set; }

    public string Unit { get; set; }

    public decimal? QuantityHold { get; set; }

    public string SubunitQty { get; set; }

    public string Subunit { get; set; }

    public string HoldStatus { get; set; }

    public DateTime? HoldDate { get; set; }

    public DateTime? HoldUntil { get; set; }

    public string HoldBy { get; set; }

    public DateTime? ReleasedDate { get; set; }

    public string ReleasedBy { get; set; }

    public string ProcessId { get; set; }

    public string Remarks { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
