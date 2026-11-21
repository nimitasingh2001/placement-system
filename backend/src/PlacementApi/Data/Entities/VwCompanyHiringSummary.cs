using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class VwCompanyHiringSummary
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public int? Hires { get; set; }

    public decimal? AvgPackageLpa { get; set; }

    public decimal? MaxPackageLpa { get; set; }
}
