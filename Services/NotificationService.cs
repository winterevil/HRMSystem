using HRMSystem.DTOs;
using HRMSystem.Helpers;
using HRMSystem.Models;
using HRMSystem.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace HRMSystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(INotificationRepository repo, IEmployeeRepository employeeRepo, IHubContext<NotificationHub> hub)
        {
            _repo = repo;
            _employeeRepo = employeeRepo;
            _hub = hub;
        }

        public async Task CreateAsync(
            NotificationType type,
            string title,
            string content,
            List<int> employeeIds)
        {
            var notification = new Notification
            {
                Title = title,
                Content = content,
                Type = type,
                CreatedAt = DateTime.UtcNow.AddHours(7)
            };

            await _repo.AddNotificationAsync(notification);

            var distinctEmployeeIds = employeeIds.Distinct().ToList();

            var recipients = distinctEmployeeIds.Select(x => new NotificationRecipient
            {
                NotificationId = notification.Id,
                EmployeeId = x,
                IsRead = false
            }).ToList();

            await _repo.AddRecipientsAsync(recipients);

            foreach (var empId in distinctEmployeeIds)
            {
                await _hub.Clients
                    .Group($"user_{empId}")
                    .SendAsync("NewNotification");
            }
        }


        public async Task<List<NotificationDto>> GetMyNotificationsAsync(int employeeId)
        {
            var data = await _repo.GetByEmployeeAsync(employeeId);

            return data.Select(x => new NotificationDto
            {
                Id = x.NotificationId,
                Title = x.Notifications.Title,
                Content = x.Notifications.Content,
                Type = x.Notifications.Type.ToString(),
                IsRead = x.IsRead,
                CreatedAt = x.Notifications.CreatedAt
            }).ToList();
        }
        public async Task MarkAsReadAsync(int notificationId, int employeeId)
        {
            await _repo.MarkAsReadAsync(notificationId, employeeId);
        }

        public async Task NotifyByRolesAsync(NotificationType type, string title, string content, params string[] roles)
        {
            var employeeIds = new List<int>();

            foreach (var role in roles)
            {
                var users = await _employeeRepo.GetEmployeesByRoleAsync(role);
                employeeIds.AddRange(users.Select(x => x.Id));
            }

            await CreateAsync(type, title, content, employeeIds.Distinct().ToList());
        }
        public async Task ClearMyNotificationsAsync(int employeeId)
        {
            await _repo.ClearAllByEmployeeAsync(employeeId, employeeId);
        }

    }
}
