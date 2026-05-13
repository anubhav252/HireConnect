using HireConnect.Notification.Consumers;
using HireConnect.Notification.Services;
using HireConnect.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace HireConnect.Notification.Tests
{
    [TestFixture]
    public class NotificationConsumerTests
    {
        private Mock<IEmailService> _emailMock;
        private Mock<ILogger<JobPostedConsumer>> _loggerMock;
        private JobPostedConsumer _consumer;

        [SetUp]
        public void SetUp()
        {
            _emailMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<JobPostedConsumer>>();
            _consumer = new JobPostedConsumer(_emailMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task JobPostedConsumer_ShouldSendEmail()
        {
            // Arrange
            var contextMock = new Mock<ConsumeContext<JobPostedEvent>>();
            var message = new JobPostedEvent(
                JobId: 1,
                Title: "Software Engineer",
                CompanyName: "HireConnect",
                Location: "Remote",
                SalaryRange: "$100k - $150k",
                PostedAt: DateTime.UtcNow
            );
            contextMock.Setup(x => x.Message).Returns(message);

            // Act
            await _consumer.Consume(contextMock.Object);

            // Assert
            _emailMock.Verify(x => x.SendEmailAsync(
                It.IsAny<string>(), 
                It.Is<string>(s => s.Contains("Software Engineer")), 
                It.IsAny<string>()), Times.Once);
        }
    }
}
