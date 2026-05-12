using HireConnect.Interview.Entities;
using HireConnect.Interview.Repositories;
using HireConnect.Shared.Events;
using MassTransit;

namespace HireConnect.Interview.Services
{
    public class InterviewServiceImpl : IInterviewService
    {
        private readonly IInterviewRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public InterviewServiceImpl(IInterviewRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<JobInterview> ScheduleInterviewAsync(JobInterview interview)
        {
            interview.Status = "Scheduled";
            var saved = await _repository.SaveAsync(interview);

            await _publishEndpoint.Publish(new InterviewScheduledEvent(
                saved.InterviewId,
                saved.ApplicationId,
                saved.CandidateId,
                "candidate@example.com", // Should be fetched from Application/Profile
                saved.ScheduledAt,
                saved.Mode,
                saved.MeetLink,
                saved.Location
            ));

            return saved;
        }

        public async Task ConfirmInterviewAsync(int interviewId)
        {
            var interview = await _repository.FindByIdAsync(interviewId) ?? throw new KeyNotFoundException("Interview not found");
            interview.Status = "Confirmed";
            await _repository.UpdateAsync(interview);
        }

        public async Task<JobInterview> RescheduleInterviewAsync(int interviewId, DateTime newTime)
        {
            var interview = await _repository.FindByIdAsync(interviewId) ?? throw new KeyNotFoundException("Interview not found");
            interview.ScheduledAt = newTime;
            interview.Status = "Rescheduled";
            await _repository.UpdateAsync(interview);
            return interview;
        }

        public async Task CancelInterviewAsync(int interviewId)
        {
            var interview = await _repository.FindByIdAsync(interviewId) ?? throw new KeyNotFoundException("Interview not found");
            interview.Status = "Cancelled";
            await _repository.UpdateAsync(interview);
        }

        public Task<IEnumerable<JobInterview>> GetByApplicationIdAsync(int applicationId) =>
            _repository.FindByApplicationIdAsync(applicationId);
    }
}
