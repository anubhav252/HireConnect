using HireConnect.Interview.Entities;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Interview.Data
{
    public class InterviewDbContext : DbContext
    {
        public InterviewDbContext(DbContextOptions<InterviewDbContext> options) : base(options) { }
        public DbSet<JobInterview> Interviews { get; set; }
    }
}
