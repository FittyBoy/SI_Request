using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class Materialtype
{
    public Guid Id { get; set; }

    public string? Typename { get; set; }

    public string? Description { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }
}
