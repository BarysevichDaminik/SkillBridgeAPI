using System;
using System.Collections.Generic;

namespace SkillBridgeAPI.Models;

public partial class Userskill
{
    public long UserSkillId { get; set; }

    public long UserId { get; set; }

    public long SkillId { get; set; }

    public string? SkillType { get; set; }

    public virtual Skill Skill { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
