using System;
using System.Collections.Generic;

namespace SI24004.Models;

public partial class MaterialType
{
    public Guid Id { get; set; }

    public string TypeName { get; set; }

    public string Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
