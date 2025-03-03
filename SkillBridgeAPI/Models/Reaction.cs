using System;
using System.Collections.Generic;

namespace SkillBridgeAPI.Models;

public partial class Reaction
{
    public long ReactionId { get; set; }

    public string MessageId { get; set; }

    public string UserId { get; set; }

    public string? ReactionType { get; set; }

    public string ulid { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Message Message { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
