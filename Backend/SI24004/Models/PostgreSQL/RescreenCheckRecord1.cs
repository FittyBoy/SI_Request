using System;
using System.Collections.Generic;
namespace SI24004.Models.PostgreSQL;

/// <summary>
/// บันทึกการตรวจสอบ LOT ที่มี Status = Rescreen
/// </summary>
public partial class RescreenCheckRecord1
{
    /// <summary>
    /// Primary Key (UUID)
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
    public string? ImobileLot { get; set; }

    /// <summary>
    /// Status จาก TH Record (Rescreen)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// วันที่ประมวลผล LOT
    /// </summary>
    public DateTime DateProcess { get; set; }

    /// <summary>
    /// วันที่ตรวจสอบและบันทึก
    /// </summary>
    public DateTime? CheckDate { get; set; }

    /// <summary>
    /// ผู้ตรวจสอบ
    /// </summary>
    public string? CheckedBy { get; set; }

    /// <summary>
    /// Status จาก TH100 Record
    /// </summary>
    public string? Th100Status { get; set; }

    /// <summary>
    /// Status สุดท้ายหลังตรวจสอบ (OK, HOLD, SCRAP, Rescreen, PENDING)
    /// </summary>
    public string? FinalStatus { get; set; }

    /// <summary>
    /// สถานะการอนุมัติ (true = อนุมัติ, false = รอตรวจสอบ)
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// แหล่งที่มาของการ Approve:
    /// "TH100 Confirm" = ผ่านจาก TH100,
    /// "Approved"      = ผ่านจาก PO Check Flow,
    /// "Pending"       = ยังไม่มีข้อมูล
    /// </summary>
    public string? ApprovedSource { get; set; }

    /// <summary>
    /// หมายเหตุเพิ่มเติม
    /// </summary>
    public string? Remarks { get; set; }
}