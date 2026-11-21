using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class Student
{
    public int StudentId { get; set; }

    public string RollNo { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Course { get; set; } = null!;

    public decimal Cgpa { get; set; }

    public int GraduationYear { get; set; }

    public bool IsActive { get; set; }

    public virtual AppUser? AppUser { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
}
