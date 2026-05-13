using FluentAssertions;
using HireConnect.Application.Entities;
using HireConnect.Application.Repositories;
using HireConnect.Application.Services;
using HireConnect.Shared.Events;
using MassTransit;
using Moq;
using NUnit.Framework;

namespace HireConnect.Application.Tests
{
    [TestFixture]
    public class ApplicationServiceTests
    {
        private Mock<IApplicationRepository> _repoMock;
        private Mock<IPublishEndpoint> _publishMock;
        private ApplicationServiceImpl _service;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IApplicationRepository>();
            _publishMock = new Mock<IPublishEndpoint>();
            _service = new ApplicationServiceImpl(_repoMock.Object, _publishMock.Object);
        }

        [Test]
        public async Task SubmitApplicationAsync_WhenNotApplied_ShouldSaveAndPublish()
        {
            // Arrange
            var app = new JobApplication { JobId = 1, CandidateId = 10, CoverLetter = "Hello" };
            _repoMock.Setup(x => x.FindFirstByJobIdAndCandidateIdAsync(1, 10)).ReturnsAsync((JobApplication)null);
            _repoMock.Setup(x => x.SaveAsync(It.IsAny<JobApplication>())).ReturnsAsync(new JobApplication { ApplicationId = 100, JobId = 1, CandidateId = 10 });

            // Act
            var result = await _service.SubmitApplicationAsync(app);

            // Assert
            result.Should().NotBeNull();
            result.ApplicationId.Should().Be(100);
            _repoMock.Verify(x => x.SaveAsync(It.IsAny<JobApplication>()), Times.Once);
            _publishMock.Verify(x => x.Publish(It.IsAny<ApplicationSubmittedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
