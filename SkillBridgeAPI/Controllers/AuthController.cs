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

namespace SkillBridgeAPI.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "JwtScheme")]
    [Route("[Controller]")]
    public class AuthController(SkillbridgeContext context) : ControllerBase
    {
        SkillbridgeContext Context { get; } = context;

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IResult> Check([FromForm] UserCredentialsWithHash creds)
        {
            User? user = await Context.Users.FirstOrDefaultAsync(u => u.Username!.Equals(creds.Username));
            if (user == null) { return Results.NotFound(); }

            bool isValid = user.PwdHash.Equals(creds.Hash, StringComparison.OrdinalIgnoreCase);
            if (!isValid) return Results.Unauthorized();

            var JWTtoken = TokenService.CreateJWTToken(user.Ulid);
            string newRefreshToken = TokenService.CreateRefreshToken();

            RefreshToken? foundUser = await Context.RefreshToken.FirstOrDefaultAsync(r => r.UserId == user.Ulid);
            if (foundUser is null)
            {
                await Context.RefreshToken.AddAsync(new RefreshToken()
                {
                    Token = newRefreshToken,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(7),
                    UserId = user.Ulid
                });
                await Context.SaveChangesAsync();
            }
            else
            {
                foundUser.Token = newRefreshToken;
                foundUser.ExpiredAt = DateTimeOffset.UtcNow.AddDays(7);
                await Context.SaveChangesAsync();
            }

            Response.Cookies.Append("jwtToken", JWTtoken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Results.Ok();
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IResult> Post([FromBody] UserCredentialsWithPwd creds)
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
                    Ulid = Convert.ToHexString(Encoding.UTF8.GetBytes(Ulid.NewUlid().ToString()))
                };
                if (!user.IsValid()) return Results.BadRequest();

                await Context.Users.AddAsync(user);
                await Context.SaveChangesAsync();

                var token = TokenService.CreateJWTToken(user.Ulid);

                return Results.Ok();
            }
            return Results.BadRequest();
        }

        [HttpPost("refreshToken")]
        public async Task<IResult> Get([FromForm] string refreshToken)
        {
            RefreshToken? token = await Context.RefreshToken.FirstOrDefaultAsync(r => r.Token == refreshToken && r.ExpiredAt > DateTimeOffset.UtcNow.ToUniversalTime());
            if (token is null) return Results.Unauthorized();
            token.Token = TokenService.CreateRefreshToken();
            token.ExpiredAt = DateTimeOffset.UtcNow.AddDays(7);
            await Context.SaveChangesAsync();

            string JWTtoken = TokenService.CreateJWTToken(token.UserId);

            Response.Cookies.Append("jwtToken", JWTtoken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            Response.Cookies.Append("refreshToken", token.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Results.Ok();
        }

        [HttpPatch("reset")]
        public async Task<IResult> Put([FromForm] string pwd)
        {
            var subClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            await Context.Users.ExecuteUpdateAsync(u =>
                u.SetProperty(u => u.PwdHash, Convert.ToHexString(SHA3_512.HashData(Encoding.UTF8.GetBytes(pwd))))
                .SetProperty(u => u.UpdatedAt, DateTime.UtcNow));
            return Results.Ok();
        }

        [HttpDelete("delete")]
        public async Task<IResult> Delete()
        {
            var subClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            await Context.Users.Where(u => u.Ulid == subClaim).ExecuteDeleteAsync();
            return Results.Ok();
        }
    }
}
