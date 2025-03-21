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
        public override async Task OnConnectedAsync()
        {
            var chatId = Context.GetHttpContext()?.Request.Query["chatId"];
            _ = long.TryParse(chatId, out long longChatId);

            if (!string.IsNullOrEmpty(chatId))
            {
                long? exchangeId = (await DBContext.Chats.FirstOrDefaultAsync(c => c.ChatId == longChatId))?.ExchangeId;
                await Groups.AddToGroupAsync(Context.ConnectionId, exchangeId!.Value.ToString());
            }
            else return;
            await base.OnConnectedAsync();
        }
        public async Task SendMessage(string chatId, string username, string message)
        {
            if (username is not null) username = username.Replace("\"", String.Empty);
            User? user = await DBContext.Users.FirstOrDefaultAsync(u => u.Ulid == username);
            long? userId = user?.UserId;
            string? chatName = (await DBContext.Chats.FirstOrDefaultAsync(u => u.ChatId == long.Parse(chatId)))?.ChatName;
            if (userId == null || chatName == null || chatId == null) return;
            List<Message> value = default!;
            if (messages.TryGetValue(chatId, out _))
            {
                value = messages[chatId];
                if (value.Count >= 30)
                {
                    await DBContext.Messages.AddRangeAsync(value);
                    await DBContext.SaveChangesAsync();
                    messages[chatId] = [];
                }
            }
            else if (value is null)
            {
                messages.Add(chatId, []);
                value = messages[chatId];
            }
            value!.Add(new Message
            {
                ChatId = long.Parse(chatId),
                UserId = (long)userId,
                Message1 = message,
                SentDate = DateTime.UtcNow,
                MessageType = "text",
                IsRead = false,
                Ulid = Ulid.NewUlid().ToString()
            });
            await Clients.All.SendMessage(user!.Username, message, chatName, chatId);
        }
    }
}
