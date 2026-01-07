using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface INotificationRepository
    {
        Task AddNotificationAsync(Notification notification);
        Task AddRecipientsAsync(List<NotificationRecipient> recipients);
        Task<List<NotificationRecipient>> GetByEmployeeAsync(int employeeId);
        Task MarkAsReadAsync(int notificationId, int employeeId);
    }
}
