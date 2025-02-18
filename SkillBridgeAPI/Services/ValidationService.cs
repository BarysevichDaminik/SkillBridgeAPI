using SkillBridgeAPI.Models;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SkillBridgeAPI.Services
{
    public static class ValidationService
    {
        public static bool IsValid(this User user)
        {
            if (user is null) return false;
            if (user.Username is null || !IsUsernameValid(user.Username)) return false;
            if (user.Email is null || !IsEmailValid(user.Email)) return false;
            return true;
        }
        static bool IsEmailValid(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return new Regex(pattern).IsMatch(email);
        }
        static bool IsUsernameValid(string username)
        {
            if (username.Length < 3 || username.Length > 50) return false;
            string pattern = @"^[a-zA-Z][a-zA-Z0-9_]*$";
            return new Regex(pattern).IsMatch(username);
        }
    }
}
