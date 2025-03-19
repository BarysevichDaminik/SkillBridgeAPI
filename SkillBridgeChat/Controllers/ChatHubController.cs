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
    public class ChatHubController : ControllerBase
    {
        readonly IHubContext<ChatHub, IMessageHub> msgHub;
        public readonly ILogger<ChatHubController> logger;
        private readonly SkillbridgeContext DBContext;
        public ChatHubController(ILogger<ChatHubController> logger, IHubContext<ChatHub, IMessageHub> msgHub, SkillbridgeContext DBContext)
        {
            this.logger = logger;
            this.msgHub = msgHub;
            this.DBContext = DBContext;
        }
        [HttpDelete("clearMsgs")]
        public async Task<IResult> ClearMsgs([FromQuery] string chatName)
        {
            foreach (List<Message> item in ChatHub.messages.Values)
            {
                await DBContext.Messages.AddRangeAsync(item);
                ChatHub.messages[chatName] = [];
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
    }
}
