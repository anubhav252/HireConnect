using HireConnect.Application.Entities;

namespace HireConnect.Application.Services
{
    public interface IApplicationService
    {
        Task<JobApplication> SubmitApplicationAsync(JobApplication application);
        Task<IEnumerable<JobApplication>> GetByCandidateAsync(int candidateId);
        Task<IEnumerable<JobApplication>> GetByJobAsync(int jobId);
        Task<string> UpdateStatusAsync(int applicationId, string status);
        Task WithdrawApplicationAsync(int applicationId);
        Task<JobApplication> GetByIdAsync(int applicationId);
        Task<int> CountByJobAsync(int jobId);
    }
}
