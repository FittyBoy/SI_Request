using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class User1
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;

    public string? UserPassword { get; set; }

    public Guid? RoleId { get; set; }

    public bool? IsDeleted { get; set; }

    public bool? Active { get; set; }

    public string? UserName { get; set; }

    public string? UserLastname { get; set; }

    public Guid? SectionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Role? Role { get; set; }

    public virtual Section? Section { get; set; }

    // Legacy aliases for controller compatibility
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Email => null;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Name => UserName;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Password => UserPassword;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? RememberToken => null;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? EmailVerifiedAt => null;
}
