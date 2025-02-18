using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkillBridgeAPI.Models;

public partial class Chat
{
    public long ChatId { get; set; }

    public string? ChatName { get; set; }

    public long? ExchangeId { get; set; }

    public DateTime? CreatedDate { get; set; }

    [JsonIgnore]
    public virtual Exchange? Exchange { get; set; }

    [JsonIgnore]
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    [JsonIgnore]
    public virtual ICollection<Exchange> Exchanges { get; set; } = new List<Exchange>();
}
