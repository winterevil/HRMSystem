using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        private readonly DeletedDbContext _deletedContext;

        public NotificationRepository(AppDbContext context, DeletedDbContext deletedContext)
        {
            _context = context;
            _deletedContext = deletedContext;
        }    
        public async Task AddNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task AddRecipientsAsync(List<NotificationRecipient> recipients)
        {
            _context.NotificationRecipients.AddRange(recipients);
            await _context.SaveChangesAsync();
        }

        public async Task<List<NotificationRecipient>> GetByEmployeeAsync(int employeeId)
        {
            return await _context.NotificationRecipients
                .Include(x => x.Notifications)
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.Notifications.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, int employeeId)
        {
            var item = await _context.NotificationRecipients
                .FirstOrDefaultAsync(x =>
                x.NotificationId == notificationId && x.EmployeeId == employeeId);

            if (item == null) return;

            item.IsRead = true;
            item.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        public async Task ClearAllByEmployeeAsync(int employeeId, int deletedById)
        {
            var items = await _context.NotificationRecipients
                .Include(x => x.Notifications)
                .Where(x => x.EmployeeId == employeeId)
                .ToListAsync();

            if (!items.Any()) return;

            var deletedNotifications = items
                .Select(x => new DeletedNotification
                {
                    Id = x.Notifications.Id,
                    Title = x.Notifications.Title,
                    Content = x.Notifications.Content,
                    Type = (int)x.Notifications.Type,
                    CreatedAt = x.Notifications.CreatedAt,
                    DeletedById = deletedById,
                    DeletedAt = DateTime.UtcNow.AddHours(7)
                })
                .DistinctBy(x => x.Id)
                .ToList();

            var deletedRecipients = items.Select(x => new DeletedNotificationRecipient
            {
                Id = x.Id,
                NotificationId = x.NotificationId,
                EmployeeId = x.EmployeeId,
                IsRead = x.IsRead,
                ReadAt = x.ReadAt,
                DeletedById = deletedById,
                DeletedAt = DateTime.UtcNow.AddHours(7)
            }).ToList();

            _deletedContext.DeletedNotifications.AddRange(deletedNotifications);
            _deletedContext.DeletedNotificationRecipients.AddRange(deletedRecipients);

            _context.NotificationRecipients.RemoveRange(items);

            await _context.SaveChangesAsync();
        }

    }
}
