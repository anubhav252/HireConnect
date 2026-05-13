using HireConnect.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Application.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<JobApplication> Applications { get; set; }
    }
}
