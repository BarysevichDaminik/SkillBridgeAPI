using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SkillBridgeChat.Models;
using SkillBridgeChat.Source;

namespace SkillBridgeChat.Hubs
{
    public class ChatHub : Hub<IMessageHub>
    {
        private readonly SkillbridgeContext DBContext;
        private static Dictionary<string, List<Message>> messages = [];
        public ChatHub(SkillbridgeContext _skillbridgeContext)
        {
            this.DBContext = _skillbridgeContext;
        }
        public async Task SendMessage(string chatName, string username, string message)
        {
            long? userId = (await DBContext.Users.FirstOrDefaultAsync(u => u.Username == username))?.UserId;
            long? chatId = (await DBContext.Chats.FirstOrDefaultAsync(u => u.ChatName == chatName))?.ChatId;
            if (userId == null || chatId == null) return;

            if (messages.TryGetValue(chatName, out List<Message>? value) && value.Count >= 30)
            {
                await DBContext.Messages.AddRangeAsync(value);
                value = [];
            }
            else if(value is null)
            {
                messages.Add(chatName, []);
                value = messages[chatName];
            }
            value!.Add(new Message
            {
                ChatId = (long)chatId,
                UserId = (long)userId,
                Message1 = message,
                SentDate = DateTime.UtcNow,
                MessageType = "text",
                IsRead = false
            });
            await Clients.All.SendMessage(message);
        }
        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            foreach(List<Message> item in messages.Values)
            {
                await DBContext.Messages.AddRangeAsync(item);
            }
            await DBContext.SaveChangesAsync();
            await base.OnDisconnectedAsync(exception);
        }
    }
}
