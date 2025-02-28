using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridgeAPI.Models;

namespace SkillBridgeAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainPageController(SkillbridgeContext Context) : ControllerBase
    {
        [HttpGet("getAvatar")]
        public async Task<IResult> GetAvatar()
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.NotFound();
            byte avatar = await Context.Users
                .Where(u => u.Ulid == user)
                .Select(u => u.AvatarNumber)
                .SingleOrDefaultAsync();
            if (avatar is 0) return Results.NotFound();
            return Results.Ok(avatar);
        }
    }
}
