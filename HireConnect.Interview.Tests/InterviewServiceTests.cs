using FluentAssertions;
using HireConnect.Interview.Entities;
using HireConnect.Interview.Repositories;
using HireConnect.Interview.Services;
using HireConnect.Shared.Events;
using MassTransit;
using Moq;
using NUnit.Framework;

namespace HireConnect.Interview.Tests
{
    [TestFixture]
    public class InterviewServiceTests
    {
        private Mock<IInterviewRepository> _repoMock;
        private Mock<IPublishEndpoint> _publishMock;
        private InterviewServiceImpl _service;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IInterviewRepository>();
            _publishMock = new Mock<IPublishEndpoint>();
            _service = new InterviewServiceImpl(_repoMock.Object, _publishMock.Object);
        }

        [Test]
        public async Task ScheduleInterviewAsync_ShouldSaveAndPublish()
        {
            // Arrange
            var interview = new JobInterview 
            { 
                ApplicationId = 1, 
                ScheduledAt = DateTime.UtcNow.AddDays(2),
                Mode = "Online",
                MeetLink = "http://meet.com"
            };
            _repoMock.Setup(x => x.SaveAsync(It.IsAny<JobInterview>())).ReturnsAsync(new JobInterview { InterviewId = 500, ApplicationId = 1, Status = "Scheduled" });

            // Act
            var result = await _service.ScheduleInterviewAsync(interview);

            // Assert
            result.Should().NotBeNull();
            result.InterviewId.Should().Be(500);
            _repoMock.Verify(x => x.SaveAsync(It.IsAny<JobInterview>()), Times.Once);
            _publishMock.Verify(x => x.Publish(It.IsAny<InterviewScheduledEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
