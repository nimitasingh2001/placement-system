using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class Company
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? Industry { get; set; }

    public string Location { get; set; } = null!;

    public string Website { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
