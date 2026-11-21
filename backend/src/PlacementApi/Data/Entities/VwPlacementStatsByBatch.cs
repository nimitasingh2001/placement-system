using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class VwPlacementStatsByBatch
{
    public int GraduationYear { get; set; }

    public int? TotalStudents { get; set; }

    public int? PlacedCount { get; set; }

    public decimal? PlacementPct { get; set; }
}
