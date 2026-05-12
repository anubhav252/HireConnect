using HireConnect.Shared.Events;
using HireConnect.Notification.Services;
using MassTransit;

namespace HireConnect.Notification.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<UserRegisteredConsumer> _logger;

        public UserRegisteredConsumer(IEmailService emailService, ILogger<UserRegisteredConsumer> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("User Registered: {Email} as {Role}", msg.Email, msg.Role);

            await _emailService.SendEmailAsync(msg.Email, "Welcome to HireConnect", 
                $"Hi {msg.FullName},\n\nWelcome to HireConnect! We're glad to have you as a {msg.Role}.");
        }
    }
}
