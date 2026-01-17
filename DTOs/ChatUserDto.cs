namespace HRMSystem.DTOs
{
    public class ChatUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; } 
        public bool CanChatDirect { get; set; }
        public bool NeedApproval { get; set; }
        public bool IsPending { get; set; }
        public int UnreadCount { get; set; }
    }
}
