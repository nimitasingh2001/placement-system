using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class Application
{
    public int ApplicationId { get; set; }

    public int StudentId { get; set; }

    public int JobId { get; set; }

    public DateOnly AppliedOn { get; set; }

    public string Status { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;

    public virtual Placement? Placement { get; set; }

    public virtual Student Student { get; set; } = null!;
}
