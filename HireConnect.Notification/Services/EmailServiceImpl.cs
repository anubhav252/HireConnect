using SendGrid;
using SendGrid.Helpers.Mail;

namespace HireConnect.Notification.Services
{
    public class EmailServiceImpl : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailServiceImpl> _logger;

        public EmailServiceImpl(IConfiguration config, ILogger<EmailServiceImpl> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("SendGrid ApiKey not found. Email to {To} was NOT sent. Body: {Body}", to, body);
                return;
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_config["SendGrid:FromEmail"] ?? "no-reply@hireconnect.com", "HireConnect");
            var toEmail = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);

            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to send email via SendGrid. Status: {Status}", response.StatusCode);
            }
            else
            {
                _logger.LogInformation("Email sent successfully to {To}", to);
            }
        }
    }
}
