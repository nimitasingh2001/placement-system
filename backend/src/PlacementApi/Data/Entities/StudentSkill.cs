using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class StudentSkill
{
    public int StudentId { get; set; }

    public int SkillId { get; set; }

    public string Proficiency { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
