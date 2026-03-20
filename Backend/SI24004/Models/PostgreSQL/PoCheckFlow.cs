using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// บันทึกการตรวจสอบ LOT ในกระบวนการผลิต
/// </summary>
public partial class PoCheckFlow
{
    /// <summary>
    /// Primary Key (UUID)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// PO LOT Number (รูปแบบ: DDMM-MC-NoPo)
    /// </summary>
    public string? PoLot { get; set; }

    /// <summary>
    /// Imobile LOT Number (Unique)
    /// </summary>
    public string? Imobilelot { get; set; }

    /// <summary>
    /// Status (OK, NG, SCRAP, HOLD, Rescreen)
    /// </summary>
    public string? StatusTn { get; set; }

    /// <summary>
    /// Check Status (true = OK, false = NG)
    /// </summary>
    public bool CheckSt { get; set; }

    /// <summary>
    /// วันที่ตรวจสอบ
    /// </summary>
    public DateTime? CheckDate { get; set; }

    /// <summary>
    /// Machine Number
    /// </summary>
    public string? McNo { get; set; }

    /// <summary>
    /// จำนวน LOT
    /// </summary>
    public int? LotQty { get; set; }
}
