namespace HRMSystem.DTOs
{
    public class ChatPermissionResult
    {
        public bool CanChat { get; set; }
        public bool NeedApproval { get; set; }
        public int? RequestId { get; set; }
    }
}
