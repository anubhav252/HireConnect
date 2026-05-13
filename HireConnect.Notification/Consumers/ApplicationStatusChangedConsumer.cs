using HireConnect.Shared.Events;
using HireConnect.Notification.Services;
using HireConnect.Notification.Entities;
using MassTransit;

namespace HireConnect.Notification.Consumers
{
    public class ApplicationStatusChangedConsumer : IConsumer<ApplicationStatusChangedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly INotificationService _notifService;
        private readonly ILogger<ApplicationStatusChangedConsumer> _logger;

        public ApplicationStatusChangedConsumer(IEmailService emailService, INotificationService notifService, ILogger<ApplicationStatusChangedConsumer> logger)
        {
            _emailService = emailService;
            _notifService = notifService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ApplicationStatusChangedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Application Status Changed: {AppId} to {Status}", msg.ApplicationId, msg.NewStatus);

            var jobTitle = msg.JobTitle ?? "a job you applied for";
            var title = $"Application Update: {msg.NewStatus}";
            var body = $"Your application status for '{jobTitle}' has been updated to: {msg.NewStatus}.";
            if (!string.IsNullOrEmpty(msg.RecruiterMessage))
            {
                body += $"\n\nMessage from recruiter: {msg.RecruiterMessage}";
            }

            // 1. Send Email (only if not a placeholder)
            if (!string.IsNullOrEmpty(msg.CandidateEmail) && msg.CandidateEmail.Contains("@") && !msg.CandidateEmail.Contains("example.com"))
            {
                await _emailService.SendEmailAsync(msg.CandidateEmail, title, body);
            }

            // 2. Save In-App Notification (only if candidate ID exists)
            if (msg.CandidateId.HasValue)
            {
                await _notifService.SendNotificationAsync(new JobNotification
                {
                    UserId = msg.CandidateId.Value,
                    Type = "ApplicationUpdate",
                    Message = body,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });
            }
        }
    }
}
