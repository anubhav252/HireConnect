using HireConnect.Shared.Events;
using HireConnect.Notification.Services;
using MassTransit;

namespace HireConnect.Notification.Consumers
{
    public class JobPostedConsumer : IConsumer<JobPostedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<JobPostedConsumer> _logger;

        public JobPostedConsumer(IEmailService emailService, ILogger<JobPostedConsumer> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<JobPostedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("New Job Posted: {Title} at {CompanyName}", msg.Title, msg.CompanyName);

            // In a real app, you might send this to all candidates interested in this category
            // For now, let's just log it or simulate a notification
            await _emailService.SendEmailAsync("yanubhav977@gmail.com", $"New Job: {msg.Title}", 
                $"A new job has been posted: {msg.Title} by {msg.CompanyName} in {msg.Location}.");
        }
    }
}
