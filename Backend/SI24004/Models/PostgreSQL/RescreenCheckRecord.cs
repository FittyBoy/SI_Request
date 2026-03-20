using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// บันทึกข้อมูล LOT ที่ผ่านการ Rescreen
/// </summary>
public partial class RescreenCheckRecord
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// LOT PO Number
    /// </summary>
    public string? LotPo { get; set; }

    /// <summary>
    /// Machine Number
    /// </summary>
    public string? McPo { get; set; }

    /// <summary>
    /// NO PO
    /// </summary>
    public string? NoPo { get; set; }

    /// <summary>
    /// Imobile LOT Number (Unique)
    /// </summary>
    public string ImobileLot { get; set; } = null!;

    /// <summary>
    /// Status: OK, HOLD, PENDING
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// วันที่บันทึก
    /// </summary>
    public DateTime DateProcess { get; set; }
}
