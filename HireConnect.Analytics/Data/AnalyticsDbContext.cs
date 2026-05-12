using HireConnect.Analytics.Entities;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Analytics.Data
{
    public class AnalyticsDbContext : DbContext
    {
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

        public DbSet<PlatformMetric> PlatformMetrics { get; set; }
        public DbSet<RecruiterMetric> RecruiterMetrics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RecruiterMetric>()
                .HasIndex(m => new { m.RecruiterId, m.MetricName })
                .IsUnique();
        }
    }
}
