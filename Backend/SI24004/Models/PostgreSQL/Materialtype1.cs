using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Materialtype1
{
    public Guid Id { get; set; }

    public string Typename { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Materalinventory> Materalinventories { get; set; } = new List<Materalinventory>();

    public virtual ICollection<Materialreceiverecord> Materialreceiverecords { get; set; } = new List<Materialreceiverecord>();
}
