using Microsoft.EntityFrameworkCore;
using SkillBridgeAPI.Models;

namespace SkillBridgeAPI.Services
{
    public class RefreshTokenService(SkillbridgeContext Context)
    {
        public async Task<List<string>> RefreshToken(HttpRequest Request = null!, string UserUlid = null!)
        {
            RefreshToken? token;
            if (UserUlid is null)
            {
                if (!Request.Cookies.TryGetValue("refreshToken", out string? _refreshToken)) return null!;
                token = await Context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == _refreshToken);
            }
            else
            {
                token = await Context.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == UserUlid);
            }
            if (token is null) return null!;

            token.Token = TokenService.CreateRefreshToken();
            token.ExpiredAt = DateTime.UtcNow.AddDays(14);
            await Context.SaveChangesAsync();

            string jwtToken = TokenService.CreateJWTToken(token.UserId);

            return [jwtToken, token.Token];
        }
    }
}
