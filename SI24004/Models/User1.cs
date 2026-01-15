using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class User1
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }

    public string Password { get; set; }

    public string RememberToken { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
