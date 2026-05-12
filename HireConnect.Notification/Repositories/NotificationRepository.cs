using HireConnect.Notification.Data;
using HireConnect.Notification.Entities;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Notification.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context) { _context = context; }

        public async Task<IEnumerable<JobNotification>> FindByUserIdAsync(int userId) =>
            await _context.Notifications.Where(n => n.UserId == userId).ToListAsync();

        public async Task<IEnumerable<JobNotification>> FindByUserIdAndIsReadAsync(int userId, bool isRead) =>
            await _context.Notifications.Where(n => n.UserId == userId && n.IsRead == isRead).ToListAsync();

        public async Task<int> CountByUserIdAndIsReadAsync(int userId, bool isRead) =>
            await _context.Notifications.CountAsync(n => n.UserId == userId && n.IsRead == isRead);

        public async Task<JobNotification> SaveAsync(JobNotification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task UpdateAsync(JobNotification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByNotificationIdAsync(int notificationId)
        {
            var notif = await _context.Notifications.FindAsync(notificationId);
            if (notif != null)
            {
                _context.Notifications.Remove(notif);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<JobNotification?> FindByIdAsync(int notificationId) =>
            await _context.Notifications.FindAsync(notificationId);
    }
}
