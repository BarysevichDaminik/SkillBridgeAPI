using System;
using System.Collections.Generic;

namespace SkillBridgeAPI.Models;

public partial class Chat
{
    public long ChatId { get; set; }

    public string? ChatName { get; set; }

    public long? ExchangeId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Exchange? Exchange { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Exchange> Exchanges { get; set; } = new List<Exchange>();
}
