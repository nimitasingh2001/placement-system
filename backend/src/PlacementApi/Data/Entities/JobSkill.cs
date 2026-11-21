using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class JobSkill
{
    public int JobId { get; set; }

    public int SkillId { get; set; }

    public bool IsMandatory { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
