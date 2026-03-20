using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Section
{
    public Guid Id { get; set; }

    public string SectionCode { get; set; }

    public string SectionName { get; set; }

    public Guid ListItemId { get; set; }

    public virtual ICollection<DwRequest> DwRequests { get; set; } = new List<DwRequest>();

    public virtual ListItem ListItem { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
