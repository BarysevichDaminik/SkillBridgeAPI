using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var token = JWTService.CreateToken(user.Ulid);
            return user.PwdHash.Equals(creds.Hash, StringComparison.OrdinalIgnoreCase) ? Results.Ok(token) : Results.Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IResult> Post([FromBody] UserCredentialsWithPwd creds)
        {
            if (creds.Username is not null && creds.Email is not null && creds.Password is not null)
            {
                User user = new()
                {
                    Email = creds.Email,
                    Username = creds.Username,
                    PwdHash = Convert.ToHexString(SHA3_512.HashData(Encoding.UTF8.GetBytes(creds.Password))),
                    Ulid = Convert.ToHexString(Encoding.UTF8.GetBytes(Ulid.NewUlid().ToString()))
                };
                if (!user.IsValid())
                    return Results.BadRequest();
                await Context.Users.AddAsync(user);
                await Context.SaveChangesAsync();
                var token = JWTService.CreateToken(user.Ulid);
                return Results.Ok(token);
            }
            return Results.BadRequest();
        }

        [HttpPut("reset")]
        public async Task<IResult> Put([FromForm] string pwd)
        {
            var subClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            await Context.Users.ExecuteUpdateAsync(u =>
                u.SetProperty(u => u.PwdHash, Convert.ToHexString(SHA3_512.HashData(Encoding.UTF8.GetBytes(pwd)))));
            return Results.Ok();
        }

        //[Authorize]
        //[HttpDelete("delete")]
        //public async Task<IResult> Delete()
        //{

        //}
    }
}
