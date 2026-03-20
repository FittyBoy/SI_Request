using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Cache
{
    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public int Expiration { get; set; }
}
