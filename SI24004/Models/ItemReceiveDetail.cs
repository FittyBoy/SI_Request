using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class ItemReceiveDetail
{
    public Guid Id { get; set; }

    public string? IssueId { get; set; }

    public string? ItemCode { get; set; }

    public string? ItemName { get; set; }

    public string? LotNumber { get; set; }

    public string? Unit { get; set; }

    public decimal? QuantityIssued { get; set; }

    public decimal? QuantityReceived { get; set; }

    public string? SubunitQty { get; set; }

    public string? Subunit { get; set; }

    public string? ReceiveStatus { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public string? ReceivedBy { get; set; }

    public string? ProcessId { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
