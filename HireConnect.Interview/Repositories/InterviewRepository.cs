using HireConnect.Interview.Data;
using HireConnect.Interview.Entities;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Interview.Repositories
{
    public class InterviewRepository : IInterviewRepository
    {
        private readonly InterviewDbContext _context;

        public InterviewRepository(InterviewDbContext context) { _context = context; }

        public async Task<IEnumerable<JobInterview>> FindByApplicationIdAsync(int applicationId) =>
            await _context.Interviews.Where(i => i.ApplicationId == applicationId).ToListAsync();

        public async Task<IEnumerable<JobInterview>> FindByStatusAsync(string status) =>
            await _context.Interviews.Where(i => i.Status == status).ToListAsync();

        public async Task<IEnumerable<JobInterview>> FindByScheduledAtBetweenAsync(DateTime start, DateTime end) =>
            await _context.Interviews.Where(i => i.ScheduledAt >= start && i.ScheduledAt <= end).ToListAsync();

        public async Task<JobInterview?> FindByIdAsync(int interviewId) =>
            await _context.Interviews.FindAsync(interviewId);

        public async Task<JobInterview> SaveAsync(JobInterview interview)
        {
            _context.Interviews.Add(interview);
            await _context.SaveChangesAsync();
            return interview;
        }

        public async Task UpdateAsync(JobInterview interview)
        {
            _context.Interviews.Update(interview);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByInterviewIdAsync(int interviewId)
        {
            var interview = await _context.Interviews.FindAsync(interviewId);
            if (interview != null)
            {
                _context.Interviews.Remove(interview);
                await _context.SaveChangesAsync();
            }
        }
    }
}
