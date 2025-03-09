namespace SkillBridgeAPI.DTOs
{
    public record MsgInfo
    {
        public long chatId { get; set; }
        public string text { get; set; }
        public string msgType { get; set; }
    }
}
