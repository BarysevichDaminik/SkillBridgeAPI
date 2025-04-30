using System;
using System.Collections.Generic;

namespace SkillBridgeChat.Models;

public partial class Exchange
{
    public long ExchangeId { get; set; }

    public long UserId1 { get; set; }

    public long UserId2 { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public long Skill1Id { get; set; }

    public long Skill2Id { get; set; }

    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();

    public virtual Skill Skill1 { get; set; } = null!;

    public virtual Skill Skill2 { get; set; } = null!;

    public virtual User UserId1Navigation { get; set; } = null!;

    public virtual User UserId2Navigation { get; set; } = null!;

    public virtual ICollection<Chat> ChatsNavigation { get; set; } = new List<Chat>();
}
