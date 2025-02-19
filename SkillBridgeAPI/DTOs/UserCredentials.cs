using Microsoft.AspNetCore.Mvc;
using SkillBridgeAPI.ModelBinders;

namespace SkillBridgeAPI.DTO
{
    public class UserCredentialsWithPwd
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
    }
    public class UserCredentialsWithHash
    {
        public string Username { get; set; }
        public string Hash { get; set; }
    }
}
