using HireConnect.Application.Data;
using HireConnect.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Application.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public ApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobApplication>> FindByCandidateIdAsync(int candidateId) =>
            await _context.Applications.Where(a => a.CandidateId == candidateId).ToListAsync();

        public async Task<IEnumerable<JobApplication>> FindByJobIdAsync(int jobId) =>
            await _context.Applications.Where(a => a.JobId == jobId).ToListAsync();

        public async Task<IEnumerable<JobApplication>> FindByStatusAsync(string status) =>
            await _context.Applications.Where(a => a.Status == status).ToListAsync();

        public async Task<JobApplication?> FindFirstByJobIdAndCandidateIdAsync(int jobId, int candidateId) =>
            await _context.Applications.FirstOrDefaultAsync(a => a.JobId == jobId && a.CandidateId == candidateId);

        public async Task<int> CountByJobIdAsync(int jobId) =>
            await _context.Applications.CountAsync(a => a.JobId == jobId);

        public async Task<JobApplication> SaveAsync(JobApplication application)
        {
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<JobApplication?> FindByIdAsync(int applicationId) =>
            await _context.Applications.FindAsync(applicationId);

        public async Task UpdateAsync(JobApplication application)
        {
            _context.Applications.Update(application);
            await _context.SaveChangesAsync();
        }
    }
}
