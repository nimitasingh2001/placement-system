using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class VwApplicationDetail
{
    public int ApplicationId { get; set; }

    public DateOnly AppliedOn { get; set; }

    public string Status { get; set; } = null!;

    public int StudentId { get; set; }

    public string RollNo { get; set; } = null!;

    public string StudentName { get; set; } = null!;

    public decimal Cgpa { get; set; }

    public int JobId { get; set; }

    public string JobTitle { get; set; } = null!;

    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;
}
