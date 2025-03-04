using System;
using System.Collections.Generic;

namespace SkillBridgeAPI.Models;

public partial class Skill
{
    public long SkillId { get; set; }

    public string SkillName { get; set; } = null!;

    public string? Category { get; set; }

    public virtual ICollection<Userskill> Userskills { get; set; } = new List<Userskill>();
}
