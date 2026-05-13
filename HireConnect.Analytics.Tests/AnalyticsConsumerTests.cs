using FluentAssertions;
using HireConnect.Analytics.Consumers;
using HireConnect.Analytics.Data;
using HireConnect.Analytics.Entities;
using HireConnect.Shared.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace HireConnect.Analytics.Tests
{
    [TestFixture]
    public class AnalyticsConsumerTests
    {
        private AnalyticsDbContext _dbContext;
        private Mock<ILogger<AnalyticsEventConsumer>> _loggerMock;
        private AnalyticsEventConsumer _consumer;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _dbContext = new AnalyticsDbContext(options);
            _loggerMock = new Mock<ILogger<AnalyticsEventConsumer>>();
            _consumer = new AnalyticsEventConsumer(_dbContext, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Consume_JobPostedEvent_ShouldIncrementTotalJobs()
        {
            // Arrange
            var contextMock = new Mock<ConsumeContext<JobPostedEvent>>();
            var message = new JobPostedEvent(1, "Title", "Company", "Location", "$", DateTime.UtcNow);
            contextMock.Setup(x => x.Message).Returns(message);

            // Act
            await _consumer.Consume(contextMock.Object);

            // Assert
            var metric = await _dbContext.PlatformMetrics.FirstOrDefaultAsync(m => m.MetricKey == "TotalJobs");
            metric.Should().NotBeNull();
            metric.Value.Should().Be(1);
        }

        [Test]
        public async Task Consume_ApplicationSubmittedEvent_ShouldIncrementTotalApplications()
        {
            // Arrange
            var contextMock = new Mock<ConsumeContext<ApplicationSubmittedEvent>>();
            var message = new ApplicationSubmittedEvent(1, 1, 1, "email", "title", DateTime.UtcNow);
            contextMock.Setup(x => x.Message).Returns(message);

            // Act
            await _consumer.Consume(contextMock.Object);

            // Assert
            var metric = await _dbContext.PlatformMetrics.FirstOrDefaultAsync(m => m.MetricKey == "TotalApplications");
            metric.Should().NotBeNull();
            metric.Value.Should().Be(1);
        }
    }
}
