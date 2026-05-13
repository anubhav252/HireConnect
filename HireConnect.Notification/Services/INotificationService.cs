using HireConnect.Notification.Entities;

namespace HireConnect.Notification.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(JobNotification notification);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllReadAsync(int userId);
        Task<IEnumerable<JobNotification>> GetByUserAsync(int userId);
        Task SendEmailAlertAsync(string to, string subject, string body);
    }
}
