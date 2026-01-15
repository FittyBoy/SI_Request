using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string UserId { get; set; }

    public string UserPassword { get; set; }

    public Guid? RoleId { get; set; }

    public bool? IsDeleted { get; set; }

    public bool? Active { get; set; }

    public string UserName { get; set; }

    public string UserLastname { get; set; }

    public Guid? SectionId { get; set; }

    public virtual ICollection<AviRequest> AviRequests { get; set; } = new List<AviRequest>();

    public virtual ICollection<DwRequest> DwRequests { get; set; } = new List<DwRequest>();

    public virtual ICollection<InaRequest> InaRequests { get; set; } = new List<InaRequest>();

    public virtual Role Role { get; set; }

    public virtual Section Section { get; set; }
}
