using Microsoft.AspNetCore.SignalR;
using SkillBridgeChat.Source;

namespace SkillBridgeChat.Hubs
{
    public class ChatHub : Hub<IMessageHub>
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendMessage(message);
        }
    }
}
