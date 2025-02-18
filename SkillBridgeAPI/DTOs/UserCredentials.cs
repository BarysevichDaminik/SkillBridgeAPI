using Microsoft.AspNetCore.Mvc;
using SkillBridgeAPI.ModelBinders;

namespace SkillBridgeAPI.DTO
{
    public class UserCredentials
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
    }
}
