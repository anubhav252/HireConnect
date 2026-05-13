using HireConnect.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Profile.Data;

public class ProfileDbContext(DbContextOptions<ProfileDbContext> options) : DbContext(options)
{
    public DbSet<CandidateProfile> CandidateProfiles { get; set; }
    public DbSet<RecruiterProfile> RecruiterProfiles { get; set; }
    public DbSet<Address>          Addresses          { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── CandidateProfile ──────────────────────────────────────────
        modelBuilder.Entity<CandidateProfile>(e =>
        {
            e.HasKey(c => c.ProfileId);
            e.HasIndex(c => c.Email).IsUnique();
            e.HasIndex(c => c.UserId).IsUnique();
            e.Property(c => c.FullName).HasMaxLength(150).IsRequired();
            e.Property(c => c.Email).HasMaxLength(255).IsRequired();
            e.Property(c => c.Mobile).HasMaxLength(20);
            e.Property(c => c.SkillsRaw).HasMaxLength(1000);
            e.Property(c => c.ResumeUrl).HasMaxLength(500);

            e.HasMany(c => c.Addresses)
             .WithOne()
             .HasForeignKey(a => a.ProfileId)
             .HasPrincipalKey(c => c.ProfileId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── RecruiterProfile ──────────────────────────────────────────
        modelBuilder.Entity<RecruiterProfile>(e =>
        {
            e.HasKey(r => r.ProfileId);
            e.HasIndex(r => r.Email).IsUnique();
            e.HasIndex(r => r.UserId).IsUnique();
            e.Property(r => r.FullName).HasMaxLength(150).IsRequired();
            e.Property(r => r.Email).HasMaxLength(255).IsRequired();
            e.Property(r => r.CompanyName).HasMaxLength(200);
            e.Property(r => r.Website).HasMaxLength(300);
            e.Property(r => r.LogoUrl).HasMaxLength(500);

            e.HasMany(r => r.Addresses)
             .WithOne()
             .HasForeignKey(a => a.ProfileId)
             .HasPrincipalKey(r => r.ProfileId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Address ───────────────────────────────────────────────────
        modelBuilder.Entity<Address>(e =>
        {
            e.HasKey(a => a.AddressId);
            e.Property(a => a.City).HasMaxLength(100);
            e.Property(a => a.State).HasMaxLength(100);
            e.Property(a => a.Pincode).HasMaxLength(20);
            e.Property(a => a.ProfileType).HasMaxLength(20);
        });
    }
}
