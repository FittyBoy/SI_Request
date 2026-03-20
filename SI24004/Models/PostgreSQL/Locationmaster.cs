using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class Locationmaster
{
    public Guid Id { get; set; }

    public string Locationcode { get; set; }

    public string Locationname { get; set; }

    public string Zone { get; set; }

    public decimal? Maxcapacity { get; set; }

    public decimal? Currentcapacity { get; set; }

    public bool? Isactive { get; set; }

    public DateOnly? Createddate { get; set; }

    public DateOnly? Updateddate { get; set; }
}
