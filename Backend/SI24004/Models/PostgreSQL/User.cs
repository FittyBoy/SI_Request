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
}
