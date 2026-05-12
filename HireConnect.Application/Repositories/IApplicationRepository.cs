using HireConnect.Application.Entities;

namespace HireConnect.Application.Repositories
{
    public interface IApplicationRepository
    {
        Task<IEnumerable<JobApplication>> FindByCandidateIdAsync(int candidateId);
        Task<IEnumerable<JobApplication>> FindByJobIdAsync(int jobId);
        Task<IEnumerable<JobApplication>> FindByStatusAsync(string status);
        Task<JobApplication?> FindFirstByJobIdAndCandidateIdAsync(int jobId, int candidateId);
        Task<int> CountByJobIdAsync(int jobId);
        Task<JobApplication> SaveAsync(JobApplication application);
        Task<JobApplication?> FindByIdAsync(int applicationId);
        Task UpdateAsync(JobApplication application);
    }
}
