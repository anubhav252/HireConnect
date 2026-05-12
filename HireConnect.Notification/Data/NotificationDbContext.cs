using HireConnect.Notification.Entities;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Notification.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }
        public DbSet<JobNotification> Notifications { get; set; }
    }
}
