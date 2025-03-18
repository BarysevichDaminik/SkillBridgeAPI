using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SkillBridgeChat.DTO;
using SkillBridgeChat.Hubs;
using SkillBridgeChat.Source;

namespace SkillBridgeChat.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatHubController : ControllerBase
    {
        readonly IHubContext<ChatHub, IMessageHub> msgHub;
        public readonly ILogger<ChatHubController> logger;
        public ChatHubController(ILogger<ChatHubController> logger, IHubContext<ChatHub, IMessageHub> msgHub)
        {
            this.logger = logger;
            this.msgHub = msgHub;
        }

        [HttpPost("msg")]
        public void msg()
        {
            msgHub.Clients.All.SendMessage("Hello, world!");
        }

        [HttpPost("subscribe")]
        public async Task ConfigureUserAsync([FromBody] UserConf userConf)
        {
            //if (!string.IsNullOrEmpty(userId))
            //{
            //    await DBContext.Users
            //        .Where(u => u.UserId == long.Parse(userId!))
            //        .ExecuteUpdateAsync(prop => prop.
            //            SetProperty(p => p.Signalrconnectionid, Context.ConnectionId));
            //}
        }
    }
}
