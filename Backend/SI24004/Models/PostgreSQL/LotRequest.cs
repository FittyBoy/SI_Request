using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class LotRequest
{
    public Guid Id { get; set; }

    public Guid RequestId { get; set; }

    public string LotNo { get; set; } = null!;

    public bool? IsDeleted { get; set; }

    public virtual InaRequest Request { get; set; } = null!;
}
