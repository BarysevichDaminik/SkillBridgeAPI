using Microsoft.AspNetCore.Mvc;
using SkillBridgeAPI.ModelBinders;

namespace SkillBridgeAPI.DTO
{
    public record UserCredentialsWithPwd
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Email { get; set; }
    }
    public record UserCredentialsWithHash
    {
        public string Username { get; set; } = null!;
        public string Hash { get; set; } = null!;
    }
}
