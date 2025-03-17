using Microsoft.AspNetCore.SignalR;
using SkillBridgeChat.Source;
using SkillBridgeChat.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace SkillBridgeChat.Hubs
{
    public class ChatHub : Hub<IMessageHub>
    {
        private readonly SkillbridgeContext DBContext;
        public ChatHub(SkillbridgeContext _skillbridgeContext) 
        { 
            this.DBContext = _skillbridgeContext;
        }
        public async Task SendMessage(string message)
        {
            await Clients.All.SendMessage(message);
        }
        public override async Task OnConnectedAsync()
        {
            var userId = (Context.GetHttpContext())?.Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                await DBContext.Users.Where(u => u.UserId == userId).ExecuteUpdateAsync(prop => prop.SetProperty(p => p.Signalrconnectionid, Context.ConnectionId));
            }

            await base.OnConnectedAsync();
        }
    }
}
