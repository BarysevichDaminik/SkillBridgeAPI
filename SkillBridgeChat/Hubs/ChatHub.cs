using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SkillBridgeChat.Models;
using SkillBridgeChat.Source;
using System.Collections.Concurrent;

namespace SkillBridgeChat.Hubs
{
    public class ChatHub(SkillbridgeContext DBContext/*, ILogger<ChatHub> logger*/) : Hub<IMessageHub>
    {
        internal static ConcurrentDictionary<string, List<Message>> messages = [];
        public override async Task OnConnectedAsync()
        {
            var chatId = Context.GetHttpContext()?.Request.Query["chatId"];
            _ = long.TryParse(chatId, out long longChatId);

            if (!string.IsNullOrEmpty(chatId))
            {
                long? exchangeId = (await DBContext.Chats.FirstOrDefaultAsync(c => c.ChatId == longChatId))?.ExchangeId;
                await Groups.AddToGroupAsync(Context.ConnectionId, exchangeId!.Value.ToString());
            }
            else { return; }
            await base.OnConnectedAsync();
        }
        public async Task SendMessage(long chatId, string username, string message)
        {
            if (username is not null) username = username.Replace("\"", String.Empty);
            User? user = await DBContext.Users.FirstOrDefaultAsync(u => u.Ulid == username);
            long? userId = user?.UserId;
            Chat? chat = await DBContext.Chats.FirstOrDefaultAsync(u => u.ChatId == chatId);
            long? exchangeId = chat?.ExchangeId;
            string? chatName = chat?.ChatName;
            if (userId == null || chatName == null || chatId == 0) return;
            List<Message> value = default!;
            if (messages.TryGetValue(chatId.ToString(), out _))
            {
                value = messages[chatId.ToString()];
                if (value.Count >= 30)
                {
                    await DBContext.Messages.AddRangeAsync(value);
                    await DBContext.SaveChangesAsync();
                    messages[chatId.ToString()] = [];
                }
            }
            else if (value is null)
            {
                messages.TryAdd(chatId.ToString(), []);
                value = messages[chatId.ToString()];
            }
            string ulid = Ulid.NewUlid().ToString();
            value!.Add(new Message
            {
                ChatId = chatId,
                UserId = (long)userId,
                Message1 = message,
                SentDate = DateTime.UtcNow,
                MessageType = "text",
                IsRead = false,
                Ulid = ulid
            });
            await Clients.Group(exchangeId.ToString()!).SendMessage(user!.Username, message, chatName, chatId, ulid);
        }
    }
}
