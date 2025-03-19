using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkillBridgeAPI.DTO;
using SkillBridgeAPI.Models;
using SkillBridgeAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace SkillBridgeAPI.Controllers
{
    [ApiController] 
    [Route("[Controller]")]
    public class AuthController(SkillbridgeContext Context, RefreshTokenService refreshTokenService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IResult> Login([FromForm] UserCredentialsWithHash creds)
        {
            User? user = await Context.Users.FirstOrDefaultAsync(u => u.Username!.Equals(creds.Username));
            if (user == null) return Results.NotFound();

            if(user.LoginAttempts == 3)
            {
                if (user.NextAttemptAt >= DateTimeOffset.UtcNow)
                {
                    return Results.BadRequest("You have entered incorect password for 3 times. Please wait 1 minute and then try again.");
                }
                else
                {
                    user.LoginAttempts = 0;
                    user.NextAttemptAt = null;
                    await Context.SaveChangesAsync();
                }
            }

            bool isValid = user.PwdHash.Equals(creds.Hash, StringComparison.OrdinalIgnoreCase);
            if (!isValid)
            {
                user.LoginAttempts++;
                if(user.LoginAttempts == 3)
                {
                    user.NextAttemptAt = DateTime.UtcNow.AddMinutes(1);
                    await Context.SaveChangesAsync();
                    return Results.BadRequest("You have entered incorect password for 3 times. Please wait 1 minute and then try again.");
                }
                await Context.SaveChangesAsync();
                return Results.Unauthorized();
            }
            user.LoginAttempts = 0;

            var tokens = await refreshTokenService.RefreshToken(UserUlid: user.Ulid);
            Response.Cookies.Append("jwtToken", tokens[0], new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            Response.Cookies.Append("refreshToken", tokens[1], new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            return Results.Ok(user.UserId);
        }

        [HttpPost("register")]
        public async Task<IResult> Register([FromBody] UserCredentialsWithPwd creds)
        {
            if (creds.Username is not null && creds.Email is not null && creds.Password is not null)
            {
                if(await Context.Users.AnyAsync(u => u.Email == creds.Email))
                    return Results.BadRequest("User with such an email is already registered.");

                User user = new()
                {
                    Email = creds.Email,
                    Username = creds.Username,
                    PwdHash = Convert.ToHexString(SHA3_512.HashData(Encoding.UTF8.GetBytes(creds.Password))),
                    Ulid = Convert.ToHexString(Encoding.UTF8.GetBytes(Ulid.NewUlid().ToString())),
                    AvatarNumber = (byte)RandomNumberGenerator.GetInt32(1, 17)
                };
                if (!user.IsValid()) return Results.BadRequest();

                await Context.Users.AddAsync(user);
                await Context.SaveChangesAsync();

                var JWTtoken = TokenService.CreateJWTToken(user.Ulid);
                string newRefreshToken = TokenService.CreateRefreshToken();

                await Context.RefreshTokens.AddAsync(new RefreshToken()
                {
                    Token = newRefreshToken,
                    ExpiredAt = DateTime.UtcNow.AddDays(14),
                    UserId = user.Ulid
                });
                await Context.SaveChangesAsync();

                Response.Cookies.Append("jwtToken", JWTtoken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

                Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

                return Results.Ok();
            }
            return Results.BadRequest();
        }

        [HttpPost("authWithToken")]
        public async Task<IResult> CheckToken()
        {
            string? ulid = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(ulid)) return Results.BadRequest();
            string? username = (await Context.Users.FirstOrDefaultAsync(u => u.Ulid == ulid))?.Username;
            if (string.IsNullOrEmpty(username)) return Results.BadRequest();
            var result = new { id = ulid, name = username};
            return HttpContext.User.Identity!.IsAuthenticated ? Results.Ok(result) : Results.Unauthorized();
        }

        [HttpPatch("reset")]
        public async Task<IResult> ResetPWD([FromForm] string pwd)
        {
            var subClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (subClaim == null) return Results.BadRequest();
            await Context.Users.ExecuteUpdateAsync(u =>
                u.SetProperty(u => u.PwdHash, Convert.ToHexString(SHA3_512.HashData(Encoding.UTF8.GetBytes(pwd))))
                .SetProperty(u => u.UpdatedAt, DateTime.UtcNow));
            return Results.Ok();
        }

        [HttpDelete("delete")]
        public async Task<IResult> DeleteUser()
        {
            var subClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            await Context.Users.Where(u => u.Ulid == subClaim).ExecuteDeleteAsync();
            await Context.RefreshTokens.Where(r => r.UserId == subClaim).ExecuteDeleteAsync();
            return Results.Ok();
        }
    }
}
