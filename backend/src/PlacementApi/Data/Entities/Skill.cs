using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class Skill
{
    public int SkillId { get; set; }

    public string SkillName { get; set; } = null!;

    public string? Category { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();

    public virtual ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
}
