using System;
using System.Collections.Generic;

namespace SkillBridgeChat.Models;

public partial class Message
{
    public long MessageId { get; set; }

    public long ChatId { get; set; }

    public long UserId { get; set; }

    public string? Message1 { get; set; }

    public DateTime? SentDate { get; set; }

    public string? MessageType { get; set; }

    public string? FileUrl { get; set; }

    public bool? IsRead { get; set; }

    public string Ulid { get; set; } = null!;

    public virtual Chat Chat { get; set; } = null!;

    public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

    public virtual User User { get; set; } = null!;
}
