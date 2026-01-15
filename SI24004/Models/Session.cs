using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class Session
{
    public string Id { get; set; }

    public long? UserId { get; set; }

    public string IpAddress { get; set; }

    public string UserAgent { get; set; }

    public string Payload { get; set; }

    public int LastActivity { get; set; }
}
