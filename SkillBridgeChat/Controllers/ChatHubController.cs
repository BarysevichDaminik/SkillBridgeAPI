using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SkillBridgeChat.DTO;
using SkillBridgeChat.Hubs;
using SkillBridgeChat.Source;
using SkillBridgeChat.Models;

namespace SkillBridgeChat.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatHubController(/*ILogger<ChatHubController> logger,*/ SkillbridgeContext DBContext) : ControllerBase
    {
        [HttpDelete("clearMsgs")]
        public async Task<IResult> ClearMessages([FromBody] MsgToDeleteDTO chatId)
        {
            foreach (List<Message> item in ChatHub.messages.Values)
            {
                await DBContext.Messages.AddRangeAsync(item);
                ChatHub.messages[chatId.ChatId.ToString()] = [];
            }
            try
            {
                await DBContext.SaveChangesAsync();
                return Results.Ok();
            }
            catch (Exception)
            {
                return Results.InternalServerError();
            }
        }

        [HttpDelete("delMsg")]
        public async Task<IResult> DeleteMessage([FromBody] MsgToDeleteDTO msgToDeleteDTO)
        {
            Chat? chat = await DBContext.Chats
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ChatId == msgToDeleteDTO.ChatId);
            if (chat is null) return Results.BadRequest();
            int result = await DBContext.Messages
                .AsNoTracking()
                .Where(m => m.ChatId == chat.ChatId && m.Ulid == msgToDeleteDTO.Ulid)
                .ExecuteDeleteAsync();
            return result == 1 ? Results.Ok() : result == 0 ? Results.NotFound() : Results.Conflict();
        }
    }
}
