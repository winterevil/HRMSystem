using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
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
    }
}
