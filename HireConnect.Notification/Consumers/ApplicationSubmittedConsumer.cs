using HireConnect.Shared.Events;
using HireConnect.Notification.Services;
using MassTransit;
using HireConnect.Notification.Entities;

namespace HireConnect.Notification.Consumers
{
    public class ApplicationSubmittedConsumer : IConsumer<ApplicationSubmittedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly INotificationService _notifService;
        private readonly ILogger<ApplicationSubmittedConsumer> _logger;

        public ApplicationSubmittedConsumer(IEmailService emailService, INotificationService notifService, ILogger<ApplicationSubmittedConsumer> logger)
        {
            _emailService = emailService;
            _notifService = notifService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ApplicationSubmittedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("New Application Submitted: {JobId} by {CandidateId}", msg.JobId, msg.CandidateId);
    
            var jobTitle = msg.JobTitle ?? "one of your job postings";
            var body = $"A new application has been submitted for '{jobTitle}' by {msg.CandidateEmail}.";
    
            // 1. Email to Recruiter (only if valid)
            // In a real app, you'd fetch the recruiter's email. Using a placeholder for now.
            // await _emailService.SendEmailAsync("recruiter@hireconnect.com", "New Application Received", body);
    
            // 2. In-App for Recruiter
            await _notifService.SendNotificationAsync(new JobNotification
            {
                UserId = msg.RecruiterId,
                Type = "NewApplication",
                Message = $"New applicant for '{jobTitle}': {msg.CandidateEmail}",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
    
            // 3. Candidate confirmation email
            if (!string.IsNullOrEmpty(msg.CandidateEmail) && msg.CandidateEmail.Contains("@") && !msg.CandidateEmail.Contains("example.com"))
            {
                await _emailService.SendEmailAsync(msg.CandidateEmail, "Application Received", 
                    $"Your application for '{jobTitle}' has been received successfully.");
            }
        }
    }
}
