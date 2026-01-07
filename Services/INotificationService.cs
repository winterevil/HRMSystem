using HRMSystem.DTOs;
using HRMSystem.Models;

namespace HRMSystem.Services
{
    public interface INotificationService
    {
        Task CreateAsync(NotificationType type, string title, string content, List<int> employeeIds);
        Task MarkAsReadAsync(int notificationId, int employeeId);
        Task<List<NotificationDto>> GetMyNotificationsAsync(int employeeId);
        Task NotifyByRolesAsync(NotificationType type, string title, string content, params string[] roles);
    }
}
