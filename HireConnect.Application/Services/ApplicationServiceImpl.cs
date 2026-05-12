using HireConnect.Application.Entities;
using HireConnect.Application.Repositories;
using HireConnect.Shared.Events;
using MassTransit;

namespace HireConnect.Application.Services
{
    public class ApplicationServiceImpl : IApplicationService
    {
        private readonly IApplicationRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public ApplicationServiceImpl(IApplicationRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<JobApplication> SubmitApplicationAsync(JobApplication application)
        {
            var existing = await _repository.FindFirstByJobIdAndCandidateIdAsync(application.JobId, application.CandidateId);
            if (existing != null) throw new InvalidOperationException("Application already submitted.");
            application.AppliedAt = DateTime.UtcNow;
            application.Status = "Applied";
            var saved = await _repository.SaveAsync(application);

            await _publishEndpoint.Publish(new ApplicationSubmittedEvent(
                saved.ApplicationId,
                saved.JobId,
                saved.RecruiterId,
                saved.CandidateId,
                saved.CandidateEmail,
                saved.JobTitle,
                saved.AppliedAt
            ));

            return saved;
        }

        public Task<IEnumerable<JobApplication>> GetByCandidateAsync(int candidateId) => _repository.FindByCandidateIdAsync(candidateId);
        public Task<IEnumerable<JobApplication>> GetByJobAsync(int jobId) => _repository.FindByJobIdAsync(jobId);
        
        public async Task<string> UpdateStatusAsync(int applicationId, string status)
        {
            var app = await _repository.FindByIdAsync(applicationId) ?? throw new KeyNotFoundException("Application not found.");
            var oldStatus = app.Status;
            app.Status = status;
            await _repository.UpdateAsync(app);

            await _publishEndpoint.Publish(new ApplicationStatusChangedEvent(
                app.ApplicationId,
                app.RecruiterId,
                app.CandidateId,
                status,
                oldStatus,
                app.CandidateEmail,
                app.JobTitle,
                ""
            ));

            return status;
        }

        public async Task WithdrawApplicationAsync(int applicationId)
        {
            var app = await _repository.FindByIdAsync(applicationId) ?? throw new KeyNotFoundException("Application not found.");
            app.Status = "Withdrawn";
            await _repository.UpdateAsync(app);
        }

        public async Task<JobApplication> GetByIdAsync(int applicationId) =>
            await _repository.FindByIdAsync(applicationId) ?? throw new KeyNotFoundException("Application not found.");

        public Task<int> CountByJobAsync(int jobId) => _repository.CountByJobIdAsync(jobId);
    }
}
