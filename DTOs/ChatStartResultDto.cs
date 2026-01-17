namespace HRMSystem.DTOs
{
    public class ChatStartResultDto
    {
        public bool CanChat { get; set; }
        public bool NeedApproval { get; set; }
        public int? ConversationId { get; set; }
        public int? RequestId { get; set; }
    }
}
