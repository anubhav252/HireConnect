using HireConnect.Shared.Events;
using HireConnect.Notification.Services;
using HireConnect.Notification.Entities;
using MassTransit;

namespace HireConnect.Notification.Consumers
{
    public class InterviewScheduledConsumer : IConsumer<InterviewScheduledEvent>
    {
        private readonly IEmailService _emailService;
        private readonly INotificationService _notifService;
        private readonly ILogger<InterviewScheduledConsumer> _logger;

        public InterviewScheduledConsumer(IEmailService emailService, INotificationService notifService, ILogger<InterviewScheduledConsumer> logger)
        {
            _emailService = emailService;
            _notifService = notifService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<InterviewScheduledEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Interview Scheduled for App: {AppId}", msg.ApplicationId);
    
            var title = "Interview Scheduled!";
            var body = $"An interview has been scheduled for {msg.ScheduledAt:f}.\nMode: {msg.Mode}\nLink: {msg.MeetLink}";
    
            // 1. Email (only if valid)
            if (!string.IsNullOrEmpty(msg.CandidateEmail) && msg.CandidateEmail.Contains("@") && !msg.CandidateEmail.Contains("example.com"))
            {
                await _emailService.SendEmailAsync(msg.CandidateEmail, title, body);
            }
    
            // 2. In-App (only if candidate ID exists)
            if (msg.CandidateId.HasValue)
            {
                await _notifService.SendNotificationAsync(new JobNotification
                {
                    UserId = msg.CandidateId.Value,
                    Type = "InterviewScheduled",
                    Message = $"Interview scheduled for {msg.ScheduledAt:f}. Click to view details.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });
            }
        }
    }
}
