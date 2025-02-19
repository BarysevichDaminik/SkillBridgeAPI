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
    public class SignController : ControllerBase
    {
        SkillbridgeContext Context { get; set; }
        public SignController(SkillbridgeContext context)
        {
            Context = context;
        }

        [HttpPost("/check")]
        public async Task<IResult> Check([FromForm] UserCredentialsWithHash creds)
        {
            User? user = await Context.Users.FirstOrDefaultAsync(u => u.Username!.Equals(creds.Username));
            if (user == null) { return Results.NotFound(); }
            return user.PwdHash.Equals(creds.Hash, StringComparison.OrdinalIgnoreCase) ? Results.Ok() : Results.Unauthorized();
        }

        [HttpPost("/addUser")]
        public async Task<IResult> Post([FromBody] UserCredentialsWithPwd creds)
        {
            if (creds.Username is not null && creds.Email is not null && creds.Password is not null)
            {
                User user = new User()
                {
                    Email = creds.Email,
                    Username = creds.Username,
                    PwdHash = BitConverter.ToString(SHA3_512.HashData(Encoding.UTF8.GetBytes(creds.Password))).Replace("-", "")
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
