using HireConnect.Notification.Entities;
using HireConnect.Notification.Repositories;

namespace HireConnect.Notification.Services
{
    public class NotificationServiceImpl : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationServiceImpl(INotificationRepository repository) { _repository = repository; }

        public async Task SendNotificationAsync(JobNotification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;
            await _repository.SaveAsync(notification);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notif = await _repository.FindByIdAsync(notificationId);
            if (notif != null)
            {
                notif.IsRead = true;
                await _repository.UpdateAsync(notif);
            }
        }

        public async Task MarkAllReadAsync(int userId)
        {
            var notifs = await _repository.FindByUserIdAndIsReadAsync(userId, false);
            foreach (var notif in notifs)
            {
                notif.IsRead = true;
                await _repository.UpdateAsync(notif);
            }
        }

        public Task<IEnumerable<JobNotification>> GetByUserAsync(int userId) =>
            _repository.FindByUserIdAsync(userId);

        public Task SendEmailAlertAsync(string to, string subject, string body)
        {
            // Placeholder for SendGrid logic
            return Task.CompletedTask;
        }
    }
}
