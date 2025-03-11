namespace SkillBridgeChat.Source
{
    public interface IMessageHub
    {
        Task SendMessage(string message);
    }
}
