using System;
using System.Collections.Generic;

namespace SkillBridgeAPI.Models;

public partial class Exchange
{
    public long ExchangeId { get; set; }

    public string UserId1 { get; set; }

    public string UserId2 { get; set; }

    public string SkillId1 { get; set; }

    public string SkillId2 { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string ulid { get; set; }

    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();

    public virtual Skill SkillId1Navigation { get; set; } = null!;

    public virtual Skill SkillId2Navigation { get; set; } = null!;

    public virtual User UserId1Navigation { get; set; } = null!;

    public virtual User UserId2Navigation { get; set; } = null!;

    public virtual ICollection<Chat> ChatsNavigation { get; set; } = new List<Chat>();
}
