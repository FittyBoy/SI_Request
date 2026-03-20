using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class License
{
    public Guid Id { get; set; }

    public string LicenseCode { get; set; } = null!;

    public string LicenseName { get; set; } = null!;

    public string? Department { get; set; }

    public string? Section { get; set; }

    public string? Process { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? SectionId { get; set; }

    public Guid? ProcessId { get; set; }

    public string? Criteria { get; set; }

    public int? Rev { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Department2? DepartmentNavigation { get; set; }

    public virtual ICollection<LicenseExam> LicenseExams { get; set; } = new List<LicenseExam>();

    public virtual ICollection<LicenseMaterial> LicenseMaterials { get; set; } = new List<LicenseMaterial>();

    public virtual ICollection<LicenseSo> LicenseSos { get; set; } = new List<LicenseSo>();

    public virtual Process? ProcessNavigation { get; set; }

    public virtual Section? SectionNavigation { get; set; }
}
