using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridgeAPI.Models;
using System;

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

        [HttpPost("establishExchange")]
        public async Task<IResult> AddExchange([FromForm] long SecondUser)
        {
            string? userUlid = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (userUlid is null) return Results.Unauthorized();
            long? user = (await Context.Users.FirstOrDefaultAsync(u => u.Ulid == userUlid))?.UserId;
            if (user is null) return Results.BadRequest();

            if (!(await Context.Users.AnyAsync(u => u.UserId == SecondUser))) return Results.BadRequest();

            // тут нужно добавить отправку запросы на добавление второму юзеру

            Exchange exchange = new()
            {
                UserId1 = (long)user,
                UserId2 = SecondUser,
                StartDate = DateTime.UtcNow
            };

            try
            {
                await Context.Exchanges.AddAsync(exchange);
                await Context.SaveChangesAsync();
            }
            catch
            {
                return Results.InternalServerError("Unable to create a new exchange.");
            }

            return Results.Created();
        }

        [HttpPost("addSkill")]
        public async Task<IResult> AddSkill([FromForm] int skillId)
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userID = (await Context.Users.FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userID is null) return Results.NotFound();

            if (!(await Context.Skills.AnyAsync(s => s.SkillId == skillId))) return Results.NotFound();

            Userskill skill = new()
            {
                UserId = (long)userID,
                SkillId = skillId
            };

            await Context.Userskills.AddAsync(skill);
            await Context.SaveChangesAsync();

            return Results.Created();
        }

        [HttpPost("addChat")]
        public async Task<IResult> AddChat([FromForm] long exchangeId, [FromForm] string name)
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userID = (await Context.Users.FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userID is null) return Results.NotFound();

            if (!User.Identity!.IsAuthenticated) return Results.Unauthorized();
            if(exchangeId is 0 || name is null) return Results.BadRequest();

            if (!(await Context.Exchanges.AnyAsync(e => e.ExchangeId == exchangeId))) return Results.NotFound();

            if (!(await Context.Exchanges
                .Where(e => e.UserId1 == userID || e.UserId2 == userID)
                .AnyAsync(e => e.ExchangeId == exchangeId))) return Results.BadRequest();

            Chat chat = new()
            {
                ExchangeId = exchangeId,
                ChatName = name,
                CreatedDate = DateTime.UtcNow,
            };
            try
            {
                await Context.Chats.AddAsync(chat);
                await Context.SaveChangesAsync();
            }
            catch
            {
                return Results.InternalServerError("Unable to create a new chat.");
            }
            return Results.Created();
        }

        [HttpPost("regMsg")]
        public async Task<IResult> regMsg([FromForm] long chatId, [FromForm] string text)
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userID = (await Context.Users.FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userID is null) return Results.NotFound();
            Chat? chat = await Context.Chats.FirstOrDefaultAsync(c => c.ChatId == chatId);
            if(chat is null) return Results.NotFound();
            if (chat.Exchange!.UserId1 != userID && chat.Exchange!.UserId2 != userID) return Results.BadRequest();

            Message message = new()
            {
                ChatId = chat.ChatId,
                UserId = userID,
                message = text
            };

            return Results.Created();
        }
    }
}
