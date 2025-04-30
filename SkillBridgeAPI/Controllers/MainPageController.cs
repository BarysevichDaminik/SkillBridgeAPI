using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridgeAPI.DTOs;
using SkillBridgeAPI.Models;
using System.Linq;
using System.Security.Cryptography;

namespace SkillBridgeAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainPageController(SkillbridgeContext Context, ILogger<MainPageController> logger) : ControllerBase
    {
        [HttpGet("getAvatar")]
        public async Task<IResult> GetAvatar()
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.NotFound();
            short avatar = await Context.Users
                .AsNoTracking()
                .Where(u => u.Ulid == user)
                .Select(u => u.AvatarNumber)
                .SingleOrDefaultAsync();
            if (avatar is 0) return Results.NotFound();
            return Results.Ok(avatar);
        }

        [HttpGet("getChatsInfo")]
        public async Task<IResult> GetChatsInfo()
        {
            string? userUlid = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (userUlid is null) return Results.BadRequest();
            long? userId = (await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Ulid == userUlid))?.UserId;
            if (userId is null) return Results.NotFound();

            Exchange? exchange = await Context.Exchanges
                .AsNoTracking()
                .Include(e => e.UserId1Navigation)
                .Include(e => e.UserId2Navigation)
                .FirstOrDefaultAsync(e => e.UserId1 == userId || e.UserId2 == userId);

            User? user1 = await Context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (exchange is null || user1 is null) return Results.BadRequest();

            bool isCurrentUserUser1 = exchange.UserId1 == user1.UserId;

            User? user2 = await Context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == (isCurrentUserUser1 ? exchange.UserId2 : exchange.UserId1) );

            if (user2 is null) return Results.BadRequest();

            var chats = await Context.Chats
                .AsNoTracking()
                .Include(c => c.Skill1)
                .Include(c => c.Skill2)
                .Where(c => c.ExchangeId == exchange.ExchangeId)
                .Select(c => new
                {
                    c.ChatName,
                    c.CreatedDate,
                    c.ChatId,
                    User1 = user1.Username,
                    User2 = user2.Username,
                    MySkill = c.Skill1.SkillName,
                    opponentSkill = c.Skill2.SkillName,
                    user2Avatar = user2.AvatarNumber
                })
                .ToListAsync();

            return Results.Ok(chats);
        }

        [HttpGet("getExchangesInfo")]
        public async Task<IResult> GetExchangesInfo()
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userId = (await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userId is null) return Results.NotFound();

            var exchanges = await Context.Exchanges
                .AsNoTracking()
                .Where(e => e.UserId1 == userId || e.UserId2 == userId)
                .Select(e => new
                {
                    exchangeId = e.ExchangeId,
                    User1 = e.UserId1Navigation.Username,
                    User2 = e.UserId2Navigation.Username,
                    IsActive = e.EndDate == null,
                    e.StartDate,
                    chats = e.Chats.Count,
                    user2Avatar = e.UserId1 == userId ? e.UserId2Navigation.AvatarNumber : e.UserId1Navigation.AvatarNumber
                })
                .ToListAsync();

            return Results.Ok(exchanges);
        }

        [HttpGet("getMySkills")]
        public async Task<IResult> GetMySkills()
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userId = (await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userId is null) return Results.NotFound();

            var skills = await Context.Userskills
                .AsNoTracking()
                .Where(us => us.UserId == userId)
                .Select(us => new
                {
                    skillName = us.Skill.SkillName,
                    skillCategory = us.Skill.Category,
                    skillType = us.SkillType
                })
                .ToListAsync();

            if(!skills.Any(s => s.skillType == "own"))
            {
                skills.Add(
                    new
                    {
                        skillName = "Навык не выбран!",
                        skillCategory = "",
                        skillType = "own",
                    }!
                );
            }
            if (!skills.Any(s => s.skillType == "explore"))
            {
                skills.Add(
                    new
                    {
                        skillName = "Навык не выбран!",
                        skillCategory = "",
                        skillType = "explore",
                    }!
                );
            }

            return Results.Ok(skills);
        }

        [HttpGet("getAllSkills")]
        public async Task<IResult> GetAllSkills()
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userId = (await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userId is null) return Results.NotFound();

            var skills = await Context.Skills
                .AsNoTracking()
                .ToListAsync();

            return Results.Ok(skills);
        }

        [HttpPost("establishExchange")]
        public async Task<IResult> AddExchange([FromForm] long SecondUser)
        {
            string? userUlid = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (userUlid is null) return Results.Unauthorized();
            User? user = (await Context.Users
                .AsNoTracking()
                .Include(u => u.Userskills)
                .FirstOrDefaultAsync(u => u.Ulid == userUlid));
            if (user is null) return Results.BadRequest();


            if (!(await Context.Users.AsNoTracking().AnyAsync(u => u.UserId == SecondUser)))
                return Results.BadRequest();

            Exchange exchange = new()
            {
                UserId1 = (long)user.UserId,
                UserId2 = SecondUser,
                StartDate = DateTime.UtcNow,
                Skill1Id = user.Userskills.First(us => us.SkillType == "own").SkillId,
                Skill2Id = user.Userskills.First(us => us.SkillType == "explore").SkillId
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
        public async Task<IResult> AddSkill([FromBody] SkillDTO skillDTO)
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userID = (await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userID is null) return Results.NotFound();

            if (!(await Context.Skills.AsNoTracking().AnyAsync(s => s.SkillId == skillDTO.skillId))) return Results.NotFound();

            Userskill skill = new()
            {
                UserId = (long)userID,
                SkillId = skillDTO.skillId,
                SkillType = skillDTO.skillType
            };

            await Context.Userskills.AddAsync(skill);
            await Context.SaveChangesAsync();

            return Results.Created();
        }

        [HttpPost("changeSkill")]
        public async Task<IResult> ChangeSkill([FromBody] SkillDTO skillDTO)
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userID = (await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userID is null) return Results.NotFound();

            if (!(await Context.Skills.AsNoTracking().AnyAsync(s => s.SkillId == skillDTO.skillId))) return Results.NotFound();

            Userskill? userskill = await Context.Userskills
                .FirstOrDefaultAsync(us =>
                    us.UserId == userID &&
                    us.SkillType == skillDTO.skillType
                );

            if (userskill is null)
            {
                await AddSkill(skillDTO);
                return Results.CreatedAtRoute("/addSkill");
            }
            if (userskill.SkillId == skillDTO.skillId) return Results.StatusCode(304); //Returns not modified

            userskill.SkillId = skillDTO.skillId;
            await Context.SaveChangesAsync();

            return Results.Ok();
        }

        [HttpPost("addChat")]
        public async Task<IResult> AddChat([FromBody] ChatInfo chatInfo)
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();
            long? userID = (await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Ulid == user))?.UserId;
            if (userID is null) return Results.NotFound();

            if (!User.Identity!.IsAuthenticated) return Results.Unauthorized();
            if (chatInfo.exchangeId is 0 || chatInfo.name is null) return Results.BadRequest();

            Exchange? ex = await Context.Exchanges.AsNoTracking().FirstOrDefaultAsync(e => e.ExchangeId == chatInfo.exchangeId);

            if (ex is null) return Results.NotFound();

            if (ex.UserId1 != userID && ex.UserId2 != userID) return Results.BadRequest();

            Chat chat = new()
            {
                ExchangeId = chatInfo.exchangeId,
                ChatName = chatInfo.name,
                CreatedDate = DateTime.UtcNow,
                Skill1Id = ex.Skill1Id,
                Skill2Id = ex.Skill2Id
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
                if (!chat || user is null) return Results.BadRequest();

                var messages = Context.Messages
                    .AsNoTracking()
                    .Where(m => m.ChatId == chatId)
                    .Select(m => new
                    {
                        user = m.User.Username,
                        message = m.Message1,
                        date = m.SentDate,
                        m.MessageId,
                        chatId,
                        ulid = m.Ulid
                    });

                return Results.Ok(messages);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in 'Msgs' endpoint. ChatName: {ChatName}, User: {User}", chatId, user);
                return Results.InternalServerError();
            }
        }

        [HttpGet("findExchange")]
        public async Task<IResult> FindExchange()
        {
            string? userUlid = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (userUlid is null) return Results.BadRequest();
            User? user = (await Context.Users
                .Include(u => u.Userskills)
                .FirstOrDefaultAsync(u => u.Ulid == userUlid));
            if (user is null) return Results.NotFound();

            List<long> blackList = [
                .. await Context.Exchanges.Where(u => u.UserId1 == user.UserId).Select(u => u.UserId2).ToListAsync(),
                .. await Context.Exchanges.Where(u => u.UserId2 == user.UserId).Select(u => u.UserId1).ToListAsync()
            ];

            long? skill1 = user.Userskills.First(us => us.SkillType == "own").SkillId;
            long? skill2 = user.Userskills.First(us => us.SkillType == "explore").SkillId;

            long? SearchingUser = (await Context.Users
                    .AsNoTracking()
                    .Include(u => u.Userskills)
                    .FirstOrDefaultAsync(
                        u => u.IsSearching
                        && !blackList.Contains(u.UserId)
                        && u.Userskills.First(us => us.SkillType == "own").SkillId == skill2
                        && u.Userskills.First(us => us.SkillType == "explore").SkillId == skill1
                        && u.UserId != user.UserId))?
                    .UserId;

            bool isExchangeCreated = false;

            user.IsSearching = true;
            await Context.SaveChangesAsync();

            int i = 0;
            while (i != 31)
            {
                SearchingUser = (await Context.Users
                    .AsNoTracking()
                    .Include(u => u.Userskills)
                    .FirstOrDefaultAsync(
                        u => u.IsSearching
                        && !blackList.Contains(u.UserId)
                        && u.Userskills.First(us => us.SkillType == "own").SkillId == skill2
                        && u.Userskills.First(us => us.SkillType == "explore").SkillId == skill1
                        && u.UserId != user.UserId))?
                    .UserId;

                await Context.Exchanges.AnyAsync(e => (e.UserId1 == user.UserId && e.UserId2 == SearchingUser) || (e.UserId2 == user.UserId && e.UserId1 == SearchingUser));
                
                    if (!isExchangeCreated)
                {
                    if(true)
                    {
                        return Results.Ok();
                    }
                }

                if (SearchingUser is null)
                {
                    i++;
                    Thread.Sleep(100);
                    continue;
                }
                else { break; }
            }
            user.IsSearching = false;
            await Context.SaveChangesAsync();
            if (SearchingUser == null)
                return Results.NotFound();

            return await AddExchange((long)SearchingUser!);
        }

        [HttpGet("getName")]
        public IResult GetName()
        {
            string? user = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (user is null) return Results.BadRequest();

            var names = Context.Users
                .AsNoTracking()
                .Where(u => u.Ulid == user)
                .Select(u =>
                    (u.FirstName != null || u.LastName != null) ?
                    new
                    {
                        firstName = u.FirstName,
                        lastName = u.LastName
                    } :
                    new
                    {
                        firstName = "undefined",
                        lastName = "undefined"
                    }!
                );

            return Results.Ok(names);
        }
    }
}
