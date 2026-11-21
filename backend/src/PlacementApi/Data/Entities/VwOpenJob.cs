using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class VwOpenJob
{
    public int JobId { get; set; }

    public string Title { get; set; } = null!;

    public string CompanyName { get; set; } = null!;

    public string Location { get; set; } = null!;

    public decimal MinCgpa { get; set; }

    public decimal PackageLpa { get; set; }

    public DateOnly Deadline { get; set; }
}
