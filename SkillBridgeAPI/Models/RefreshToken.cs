using System;
using System.Collections.Generic;

namespace SkillBridgeAPI.Models;

public partial class RefreshToken
{
    public long TokenId { get; set; }

    public DateTime ExpiredAt { get; set; }

    public string UserId { get; set; } = null!;

    public string Token { get; set; } = null!;
}
