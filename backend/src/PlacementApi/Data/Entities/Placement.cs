using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class Placement
{
    public int PlacementId { get; set; }

    public int ApplicationId { get; set; }

    public DateOnly PlacementDate { get; set; }

    public decimal PackageLpa { get; set; }

    public string AcademicYear { get; set; } = null!;

    public virtual Application Application { get; set; } = null!;
}
