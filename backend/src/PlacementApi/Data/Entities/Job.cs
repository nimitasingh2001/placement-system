using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class Job
{
    public int JobId { get; set; }

    public int CompanyId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Location { get; set; } = null!;

    public decimal MinCgpa { get; set; }

    public decimal PackageLpa { get; set; }

    public DateOnly Deadline { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
}
