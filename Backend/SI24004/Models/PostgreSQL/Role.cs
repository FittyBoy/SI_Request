using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Role
{
    public Guid Id { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<User1> User1s { get; set; } = new List<User1>();
}
