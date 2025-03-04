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
            short avatar = await Context.Users
                .Where(u => u.Ulid == user)
                .Select(u => u.AvatarNumber)
                .SingleOrDefaultAsync();
            if (avatar is 0) return Results.NotFound();
            return Results.Ok(avatar);
        }

        [HttpGet("getChatsInfo")]
        public async Task<IResult> GetChatsInfo()
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userID = (await Context.Users.FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userID is null) return Results.NotFound();
            var count = await Context.Chats.Where(c => c.Exchange!.UserId1 == userID || c.Exchange.UserId2 == userID).CountAsync();
            return Results.Ok(count);
        }

        //[HttpPost("establishExchange")]
        //public async Task<IResult> AddExchange([FromForm] string SecondUser)
        //{
        //    string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        //    if (user is null) return Results.BadRequest();

        //    User? secondUser = await Context.Users.FirstOrDefaultAsync(u => u.Ulid == SecondUser);
        //    if (secondUser is null) return Results.BadRequest();

        //    // тут нужно добавить отправку запросы на добавление второму юзеру

        //    Exchange exchange = new()
        //    {
        //        UserId1 = 
        //    }

        //    return Results.Ok();
        //}

        [HttpPost("addSkill")]
        public async Task<IResult> AddSkill([FromForm] string SecondUser, int skillId)
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userID = (await Context.Users.FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userID is null) return Results.NotFound();

            Userskill skill = new()
            {
                UserId = (long)userID,
                SkillId = skillId
            };

            await Context.Userskills.AddAsync(skill);
            await Context.SaveChangesAsync();

            return Results.Ok();
        }
    }
}
