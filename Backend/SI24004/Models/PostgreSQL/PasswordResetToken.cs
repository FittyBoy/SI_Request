using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class PasswordResetToken
{
    public string Email { get; set; }

    public string Token { get; set; }

    public DateTime? CreatedAt { get; set; }
}
