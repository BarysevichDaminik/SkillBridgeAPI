namespace SkillBridgeChat.Source
{
    public interface IMessageHub
    {
        Task SendMessage(string username, string message, string chatName, long chatId, string ulid);
    }
}
