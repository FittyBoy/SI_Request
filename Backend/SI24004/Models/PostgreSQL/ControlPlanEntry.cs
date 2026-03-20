using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// Control Plan entries based on Excel CONTROL_PLAN format
/// </summary>
public partial class ControlPlanEntry
{
    public int Id { get; set; }

    public int DepartmentId { get; set; }

    public string? ControlPlanNo { get; set; }

    public string? Product { get; set; }

    public string? Revision { get; set; }

    public DateOnly? RevisionDate { get; set; }

    public string? SectionName { get; set; }

    public int? EntryNo { get; set; }

    public string? ProcessEquipmentName { get; set; }

    public string? CriticalItem { get; set; }

    public string? ControlItem { get; set; }

    public string? ControlValue { get; set; }

    public string? RespPerson { get; set; }

    public string? ConfirmMethod { get; set; }

    public string? ConfirmFrequency { get; set; }

    public string? DataFormat { get; set; }

    public string? CheckSheet { get; set; }

    public string? DocumentConcern { get; set; }

    public string? PathData { get; set; }

    public string? QcControlItem { get; set; }

    public string? QcResp { get; set; }

    public string? QcConfirmMethod { get; set; }

    public string? QcFrequency { get; set; }

    public string? QcPathData { get; set; }

    public string? QcDataFormat { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Admin? CreatedByNavigation { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Admin? UpdatedByNavigation { get; set; }
}
