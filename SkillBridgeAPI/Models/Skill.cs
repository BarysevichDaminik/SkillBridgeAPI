using System;
using System.Collections.Generic;

namespace SkillBridgeAPI.Models;

public partial class Skill
{
    public long SkillId { get; set; }

    public string SkillName { get; set; } = null!;

    public string? Category { get; set; }

    public virtual ICollection<Chat> ChatSkill1s { get; set; } = new List<Chat>();

    public virtual ICollection<Chat> ChatSkill2s { get; set; } = new List<Chat>();

    public virtual ICollection<Exchange> ExchangeSkill1s { get; set; } = new List<Exchange>();

    public virtual ICollection<Exchange> ExchangeSkill2s { get; set; } = new List<Exchange>();

    public virtual ICollection<Userskill> Userskills { get; set; } = new List<Userskill>();
}
