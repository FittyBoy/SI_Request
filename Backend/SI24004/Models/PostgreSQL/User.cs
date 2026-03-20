using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class User
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime? EmailVerifiedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public bool? Active { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Legacy aliases for controller compatibility
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? UserId => null;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? UserName => Name;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? UserLastname => null;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? UserPassword => null;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Guid? RoleId => null;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Guid? SectionId => null;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Role? Role => null;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Section? Section => null;
}
