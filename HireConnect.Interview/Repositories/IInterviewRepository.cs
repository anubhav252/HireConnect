using HireConnect.Interview.Entities;

namespace HireConnect.Interview.Repositories
{
    public interface IInterviewRepository
    {
        Task<IEnumerable<JobInterview>> FindByApplicationIdAsync(int applicationId);
        Task<IEnumerable<JobInterview>> FindByStatusAsync(string status);
        Task<IEnumerable<JobInterview>> FindByScheduledAtBetweenAsync(DateTime start, DateTime end);
        Task<JobInterview?> FindByIdAsync(int interviewId);
        Task<JobInterview> SaveAsync(JobInterview interview);
        Task UpdateAsync(JobInterview interview);
        Task DeleteByInterviewIdAsync(int interviewId);
    }
}
