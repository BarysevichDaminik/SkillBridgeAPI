using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SkillBridgeAPI.Models;

public partial class User
{
    public long UserId { get; set; }

    public string Email { get; set; } = null!;

    public string PwdHash { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Username { get; set; }

    [JsonIgnore]
    public virtual ICollection<Exchange> ExchangeUserId1Navigations { get; set; } = new List<Exchange>();

    [JsonIgnore]
    public virtual ICollection<Exchange> ExchangeUserId2Navigations { get; set; } = new List<Exchange>();

    [JsonIgnore]
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    [JsonIgnore]
    public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    
    [JsonIgnore]
    public virtual ICollection<Userskill> Userskills { get; set; } = new List<Userskill>();
}
