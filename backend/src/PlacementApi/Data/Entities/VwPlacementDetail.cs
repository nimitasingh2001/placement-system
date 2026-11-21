using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class VwPlacementDetail
{
    public int PlacementId { get; set; }

    public string AcademicYear { get; set; } = null!;

    public DateOnly PlacementDate { get; set; }

    public decimal PackageLpa { get; set; }

    public int StudentId { get; set; }

    public string RollNo { get; set; } = null!;

    public string StudentName { get; set; } = null!;

    public string Course { get; set; } = null!;

    public int GraduationYear { get; set; }

    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public int JobId { get; set; }

    public string JobTitle { get; set; } = null!;
}
