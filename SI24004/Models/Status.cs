using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class Status
{
    public Guid Id { get; set; }

    public string StatusName { get; set; }

    public bool? IsDeleted { get; set; }

    public int? Ordinal { get; set; }

    public Guid? StatusTypeId { get; set; }

    public virtual ICollection<DwRequest> DwRequests { get; set; } = new List<DwRequest>();

    public virtual ICollection<InaRequest> InaRequests { get; set; } = new List<InaRequest>();

    public virtual ListItem StatusType { get; set; }
}
