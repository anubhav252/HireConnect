using HireConnect.Job.Data;
using HireConnect.Job.Middleware;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Job.Repositories
{
    /// <summary>
    /// EF Core implementation of IJobRepository.
    /// All queries are async and use the injected JobDbContext.
    /// </summary>
    public class JobRepository : IJobRepository
    {
        private readonly JobDbContext _context;
        private readonly ILogger<JobRepository> _logger;

        public JobRepository(JobDbContext context, ILogger<JobRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── Basic CRUD ──────────────────────────────────────────────────────────

        public async Task<Entities.Job?> GetByIdAsync(int id)
        {
            return await _context.Jobs.AsNoTracking()
                                      .FirstOrDefaultAsync(j => j.JobId == id);
        }

        public async Task<IEnumerable<Entities.Job>> GetAllAsync()
        {
            return await _context.Jobs.AsNoTracking()
                                      .OrderByDescending(j => j.PostedAt)
                                      .ToListAsync();
        }

        public async Task<Entities.Job> AddAsync(Entities.Job job)
        {
            job.PostedAt = DateTime.UtcNow;
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Job created: {JobId} - {Title}", job.JobId, job.Title);
            return job;
        }

        public async Task<Entities.Job> UpdateAsync(Entities.Job job)
        {
            job.UpdatedAt = DateTime.UtcNow;
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Job updated: {JobId}", job.JobId);
            return job;
        }

        public async Task DeleteAsync(int id)
        {
            var job = await _context.Jobs.FindAsync(id)
                      ?? throw new NotFoundException($"Job with ID {id} not found.");

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Job deleted: {JobId}", id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Jobs.AnyAsync(j => j.JobId == id);
        }

        // ── Filter finders ──────────────────────────────────────────────────────

        public async Task<IEnumerable<Entities.Job>> FindByTitleAsync(string title)
        {
            return await _context.Jobs.AsNoTracking()
                                      .Where(j => j.Title.ToLower().Contains(title.ToLower()))
                                      .OrderByDescending(j => j.PostedAt)
                                      .ToListAsync();
        }

        public async Task<IEnumerable<Entities.Job>> FindByCategoryAsync(string category)
        {
            return await _context.Jobs.AsNoTracking()
                                      .Where(j => j.Category.ToLower() == category.ToLower())
                                      .OrderByDescending(j => j.PostedAt)
                                      .ToListAsync();
        }

        public async Task<IEnumerable<Entities.Job>> FindByLocationAsync(string location)
        {
            return await _context.Jobs.AsNoTracking()
                                      .Where(j => j.Location.ToLower().Contains(location.ToLower()))
                                      .OrderByDescending(j => j.PostedAt)
                                      .ToListAsync();
        }

        public async Task<IEnumerable<Entities.Job>> FindByPostedByAsync(int recruiterId)
        {
            return await _context.Jobs.AsNoTracking()
                                      .Where(j => j.PostedBy == recruiterId)
                                      .OrderByDescending(j => j.PostedAt)
                                      .ToListAsync();
        }

        public async Task<IEnumerable<Entities.Job>> FindByStatusAsync(string status)
        {
            return await _context.Jobs.AsNoTracking()
                                      .Where(j => j.Status.ToLower() == status.ToLower())
                                      .OrderByDescending(j => j.PostedAt)
                                      .ToListAsync();
        }

        public async Task<IEnumerable<Entities.Job>> FindBySalaryRangeAsync(double minSalary, double maxSalary)
        {
            return await _context.Jobs.AsNoTracking()
                                      .Where(j => j.SalaryMin >= minSalary && j.SalaryMax <= maxSalary)
                                      .OrderByDescending(j => j.PostedAt)
                                      .ToListAsync();
        }

        public async Task<IEnumerable<Entities.Job>> FindByTitleContainingAsync(string keyword)
        {
            return await _context.Jobs.AsNoTracking()
                                      .Where(j => j.Title.ToLower().Contains(keyword.ToLower())
                                               || (j.Description != null && j.Description.ToLower().Contains(keyword.ToLower())))
                                      .OrderByDescending(j => j.PostedAt)
                                      .ToListAsync();
        }
    }
}
