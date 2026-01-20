using Microsoft.AspNetCore.SignalR;

namespace HRMSystem.Helpers
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
    }
}
