using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class CacheLock
{
    public string Key { get; set; }

    public string Owner { get; set; }

    public int Expiration { get; set; }
}
