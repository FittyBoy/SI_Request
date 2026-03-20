using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

/// <summary>
/// Control Plan entries based on Excel CONTROL_PLAN format
/// </summary>
public partial class ControlPlanEntry1
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

    public string? KpivCharacteristic { get; set; }

    public string? KpivSpec { get; set; }

    public string? KpivResp { get; set; }

    public string? KpivMethod { get; set; }

    public string? KpivFrequency { get; set; }

    public string? KpivDataFormat { get; set; }

    public string? KpivCheckSheet { get; set; }

    public string? KpivDocument { get; set; }

    public string? KpivPathData { get; set; }

    public string? KpovCharacteristic { get; set; }

    public string? KpovResp { get; set; }

    public string? KpovMethod { get; set; }

    public string? KpovFrequency { get; set; }

    public string? KpovPathData { get; set; }

    public string? KpovDataFormat { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Admin1? CreatedByNavigation { get; set; }

    public virtual Department1 Department { get; set; } = null!;

    public virtual Admin1? UpdatedByNavigation { get; set; }
}
