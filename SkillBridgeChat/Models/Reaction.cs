using System;
using System.Collections.Generic;

namespace SkillBridgeChat.Models;

public partial class Reaction
{
    public long ReactionId { get; set; }

    public long MessageId { get; set; }

    public long UserId { get; set; }

    public string? ReactionType { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Message Message { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
