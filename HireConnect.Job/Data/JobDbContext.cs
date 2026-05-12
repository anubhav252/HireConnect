using HireConnect.Job.Entities;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Job.Data
{
    /// <summary>
    /// EF Core DbContext for the Job-Service.
    /// Connects to PostgreSQL on Neon via Npgsql provider.
    /// </summary>
    public class JobDbContext : DbContext
    {
        public JobDbContext(DbContextOptions<JobDbContext> options) : base(options) { }

        public DbSet<Entities.Job> Jobs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Entities.Job>(entity =>
            {
                entity.ToTable("Jobs");

                entity.HasKey(j => j.JobId);

                entity.Property(j => j.JobId)
                      .UseIdentityAlwaysColumn(); // PostgreSQL IDENTITY

                entity.Property(j => j.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(j => j.Category)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(j => j.Type)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(j => j.Location)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(j => j.SalaryMin)
                      .HasColumnType("double precision")
                      .IsRequired();

                entity.Property(j => j.SalaryMax)
                      .HasColumnType("double precision")
                      .IsRequired();

                // Skills stored as raw comma-separated string column
                entity.Property(j => j.SkillsRaw)
                      .HasColumnName("Skills")
                      .HasMaxLength(1000)
                      .HasDefaultValue(string.Empty);

                // Skills is a computed/NotMapped property — exclude from mapping
                entity.Ignore(j => j.Skills);

                entity.Property(j => j.ExperienceRequired)
                      .IsRequired();

                entity.Property(j => j.PostedBy)
                      .IsRequired();

                entity.Property(j => j.Status)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasDefaultValue("Active");

                entity.Property(j => j.PostedAt)
                      .IsRequired()
                      .HasDefaultValueSql("NOW()");

                entity.Property(j => j.Description)
                      .HasMaxLength(5000);

                entity.Property(j => j.UpdatedAt)
                      .IsRequired(false);

                // Indexes for common filter queries
                entity.HasIndex(j => j.Category).HasDatabaseName("IX_Jobs_Category");
                entity.HasIndex(j => j.Location).HasDatabaseName("IX_Jobs_Location");
                entity.HasIndex(j => j.PostedBy).HasDatabaseName("IX_Jobs_PostedBy");
                entity.HasIndex(j => j.Status).HasDatabaseName("IX_Jobs_Status");
                entity.HasIndex(j => j.PostedAt).HasDatabaseName("IX_Jobs_PostedAt");
            });
        }
    }
}
