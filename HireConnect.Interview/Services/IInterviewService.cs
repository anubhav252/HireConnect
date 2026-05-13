using HireConnect.Interview.Entities;

namespace HireConnect.Interview.Services
{
    public interface IInterviewService
    {
        Task<JobInterview> ScheduleInterviewAsync(JobInterview interview);
        Task ConfirmInterviewAsync(int interviewId);
        Task<JobInterview> RescheduleInterviewAsync(int interviewId, DateTime newTime);
        Task CancelInterviewAsync(int interviewId);
        Task<IEnumerable<JobInterview>> GetByApplicationIdAsync(int applicationId);
    }
}
