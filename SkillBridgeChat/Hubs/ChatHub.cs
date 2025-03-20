using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SkillBridgeChat.Models;
using SkillBridgeChat.Source;

namespace SkillBridgeChat.Hubs
{
    public class ChatHub : Hub<IMessageHub>
    {
        private readonly SkillbridgeContext DBContext;
        public static Dictionary<string, List<Message>> messages = [];
        public ChatHub(SkillbridgeContext _skillbridgeContext)
        {
            this.DBContext = _skillbridgeContext;
        }
        public async Task SendMessage(string chatName, string username, string message)
        {
            if (username is not null) username = username.Replace("\"", String.Empty);
            User? user = await DBContext.Users.FirstOrDefaultAsync(u => u.Ulid == username);
            long? userId = user?.UserId;
            long? chatId = (await DBContext.Chats.FirstOrDefaultAsync(u => u.ChatName == chatName))?.ChatId;
            if (userId == null || chatId == null) return;
            List<Message> value = default!;
            if (messages.TryGetValue(chatName, out _))
            {
                value = messages[chatName];
                if (value.Count >= 30)
                {
                    await DBContext.Messages.AddRangeAsync(value);
                    await DBContext.SaveChangesAsync();
                    messages[chatName] = [];
                }
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
                IsRead = false,
                Ulid = Ulid.NewUlid().ToString()
            });
            await Clients.All.SendMessage(user!.Username, message, chatName);
        }
    }
}
