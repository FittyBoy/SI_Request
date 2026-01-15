using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class LotRequest
{
    public Guid Id { get; set; }

    public Guid RequestId { get; set; }

    public string LotNo { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual InaRequest Request { get; set; }
}
