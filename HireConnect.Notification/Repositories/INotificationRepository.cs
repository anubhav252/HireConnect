using HireConnect.Notification.Entities;

namespace HireConnect.Notification.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<JobNotification>> FindByUserIdAsync(int userId);
        Task<IEnumerable<JobNotification>> FindByUserIdAndIsReadAsync(int userId, bool isRead);
        Task<int> CountByUserIdAndIsReadAsync(int userId, bool isRead);
        Task<JobNotification> SaveAsync(JobNotification notification);
        Task UpdateAsync(JobNotification notification);
        Task DeleteByNotificationIdAsync(int notificationId);
        Task<JobNotification?> FindByIdAsync(int notificationId);
    }
}
