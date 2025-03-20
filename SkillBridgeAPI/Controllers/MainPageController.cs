using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridgeAPI.DTOs;
using SkillBridgeAPI.Models;

namespace SkillBridgeAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainPageController(SkillbridgeContext Context, ILogger<MainPageController> logger) : ControllerBase
    {
        static readonly string[] msgTypes = ["text", "image", "file"];

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

            var chats = await Context.Chats
                .Where(c => c.Exchange!.UserId1 == userID || c.Exchange.UserId2 == userID)
                .Select(c => new
                {
                    c.ChatName,
                    c.CreatedDate,
                    c.ChatId,
                    User1 = c.Exchange!.UserId1Navigation.Username,
                    User2 = c.Exchange!.UserId2Navigation.Username,
                    MySkill = Context.Skills.First(
                        s => s.SkillId == Context.Userskills.First(
                            u => u.UserSkillId == c.Exchange!.UserId1).SkillId).SkillName,
                    opponentSkill = Context.Skills.First(
                        s => s.SkillId == Context.Userskills.First(
                            u => u.UserSkillId == c.Exchange!.UserId2).SkillId).SkillName,
                    user2Avatar = c.Exchange!.UserId2Navigation.AvatarNumber
                })
                .ToListAsync();

            return Results.Ok(chats);
        }

        [HttpGet("getExchangesInfo")]
        public async Task<IResult> GetExchangesInfo()
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userID = (await Context.Users.FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userID is null) return Results.NotFound();

            var chats = await Context.Exchanges
                .Where(e => e.UserId1 == userID || e.UserId2 == userID)
                .Select(e => new
                {
                    User1 = e.UserId1Navigation.Username,
                    User2 = e.UserId2Navigation.Username,
                    IsActive = e.EndDate == null,
                    e.StartDate,
                    chats = e.Chats.Count,
                    user2Avatar = e.UserId2Navigation.AvatarNumber
                })
                .ToListAsync();

            return Results.Ok(chats);
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
        public async Task<IResult> AddChat([FromForm] ChatInfo chatInfo)
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userID = (await Context.Users.FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userID is null) return Results.NotFound();

            if (!User.Identity!.IsAuthenticated) return Results.Unauthorized();
            if (chatInfo.exchangeId is 0 || chatInfo.name is null) return Results.BadRequest();

            if (!(await Context.Exchanges.AnyAsync(e => e.ExchangeId == chatInfo.exchangeId))) return Results.NotFound();

            if (!(await Context.Exchanges
                .Where(e => e.UserId1 == userID || e.UserId2 == userID)
                .AnyAsync(e => e.ExchangeId == chatInfo.exchangeId))) { return Results.BadRequest(); }

            Chat chat = new()
            {
                ExchangeId = chatInfo.exchangeId,
                ChatName = chatInfo.name,
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

        [HttpGet("msgs")]
        public async Task<IResult> Msgs([FromQuery] long chatId)
        {
            string? user = "undefined";
            try
            {
                if (!User.Identity!.IsAuthenticated) return Results.Unauthorized();
                user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                bool chat = await Context.Chats.AsNoTracking().AnyAsync(c => c.ChatId == chatId);
                if (chat is false || user is null) return Results.BadRequest();

                var messages = Context.Messages
                    .AsNoTracking()
                    .Where(m => m.ChatId == chatId)
                    .Select(m => new
                    {
                        user = m.User.Username,
                        m.message,
                        date = m.SentDate,
                        chatId
                    });

                return Results.Ok(messages);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in 'Msgs' endpoint. ChatName: {ChatName}, User: {User}", chatId, user);
                return Results.InternalServerError();
            }
        }
    }
}
