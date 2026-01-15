using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class Objective
{
    public Guid Id { get; set; }

    public string ObjectName { get; set; }

    public Guid ListItemId { get; set; }

    public virtual ListItem ListItem { get; set; }
}
