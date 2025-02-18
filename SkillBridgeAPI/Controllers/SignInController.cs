using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridgeAPI.DTO;
using SkillBridgeAPI.Models;
using SkillBridgeAPI.Services;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace SkillBridgeAPI.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class SignInController : ControllerBase
    {
        SkillBridgeDbContext Context { get; set; }
        public SignInController(SkillBridgeDbContext context) 
        {
            Context = context;
        }

        [HttpPost("/check")]
        public async Task<IResult> Check([FromForm] long id, [FromForm] string hash)
        {
            User? user = await Context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) { return Results.NotFound(); }
            return user.PwdHash.Equals(hash) ? Results.Ok() : Results.Unauthorized();
        }

        [HttpPost]
        public async Task<IResult> Post([FromBody] UserCredentials creds)
        {
            if (creds.Username is not null && creds.Email is not null &&  creds.PasswordHash is not null)
            {
                User user = new User()
                {
                    Email = creds.Email,
                    Username = creds.Username,
                    PwdHash = Encoding.UTF8.GetString(SHA3_512.HashData(Encoding.UTF8.GetBytes(creds.PasswordHash)))
                };
                if (!user.IsValid()) 
                    return Results.BadRequest();
                await Context.Users.AddAsync(user);
                await Context.SaveChangesAsync();
                return Results.Ok();
            }
            return Results.BadRequest();
        }
    }
}
