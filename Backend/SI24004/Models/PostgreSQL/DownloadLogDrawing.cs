using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class DownloadLogDrawing
{
    public Guid Id { get; set; }

    public string UserEmail { get; set; }

    public string UserUsername { get; set; }

    public string IpAddress { get; set; }

    public DateOnly? CreateDate { get; set; }
}
