﻿using System;
using System.Collections.Generic;

namespace SkillBridgeChat.Models;

public partial class User
{
    public long UserId { get; set; }

    public string Email { get; set; } = null!;

    public string PwdHash { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Username { get; set; } = null!;

    public bool? SubscriptionStatus { get; set; }

    public string Ulid { get; set; } = null!;

    public short LoginAttempts { get; set; }

    public DateTime? NextAttemptAt { get; set; }

    public short AvatarNumber { get; set; }

    public short Rating { get; set; }

    public bool IsSearching { get; set; }

    public virtual ICollection<Exchange> ExchangeUserId1Navigations { get; set; } = new List<Exchange>();

    public virtual ICollection<Exchange> ExchangeUserId2Navigations { get; set; } = new List<Exchange>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

    public virtual ICollection<Userskill> Userskills { get; set; } = new List<Userskill>();
}
