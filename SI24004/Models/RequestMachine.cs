using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class RequestMachine
{
    public string? Id { get; set; }

    public string? RequestMachineName { get; set; }

    public bool? Active { get; set; }

    public bool? IsDeleted { get; set; }

    public Guid ListItemId { get; set; }

    public virtual ListItem ListItem { get; set; }
}
