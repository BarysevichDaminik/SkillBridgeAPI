using System;
using System.Collections.Generic;

namespace SkillBridgeChat.Models;

public partial class Chat
{
    public long ChatId { get; set; }

    public string ChatName { get; set; } = null!;

    public long ExchangeId { get; set; }

    public DateTime CreatedDate { get; set; }

    public long Skill1Id { get; set; }

    public long Skill2Id { get; set; }

    public virtual Exchange Exchange { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Skill Skill1 { get; set; } = null!;

    public virtual Skill Skill2 { get; set; } = null!;

    public virtual ICollection<Exchange> Exchanges { get; set; } = new List<Exchange>();
}
