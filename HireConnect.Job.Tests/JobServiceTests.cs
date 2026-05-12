using FluentAssertions;
using HireConnect.Job.DTOs;
using HireConnect.Job.Elasticsearch;
using HireConnect.Job.Entities;
using HireConnect.Job.Repositories;
using HireConnect.Job.Services;
using HireConnect.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace HireConnect.Job.Tests
{
    [TestFixture]
    public class JobServiceTests
    {
        private Mock<IJobRepository> _jobRepoMock;
        private Mock<IJobElasticsearchService> _esServiceMock;
        private Mock<IPublishEndpoint> _publishMock;
        private Mock<ILogger<JobServiceImpl>> _loggerMock;
        private JobServiceImpl _jobService;

        [SetUp]
        public void SetUp()
        {
            _jobRepoMock = new Mock<IJobRepository>();
            _esServiceMock = new Mock<IJobElasticsearchService>();
            _publishMock = new Mock<IPublishEndpoint>();
            _loggerMock = new Mock<ILogger<JobServiceImpl>>();

            _jobService = new JobServiceImpl(
                _jobRepoMock.Object, 
                _esServiceMock.Object, 
                _publishMock.Object, 
                _loggerMock.Object);
        }

        [Test]
        public async Task AddJobAsync_ShouldSaveToDb_AndIndexInES_AndPublishEvent()
        {
            // Arrange
            var dto = new JobRequestDto
            {
                Title = "Test Job",
                Description = "Test Description",
                Type = "Full-Time",
                Category = "Engineering",
                Location = "New York"
            };
            int recruiterId = 100;

            var savedJob = new Entities.Job { JobId = 1, Title = dto.Title, PostedBy = recruiterId };
            _jobRepoMock.Setup(x => x.AddAsync(It.IsAny<Entities.Job>())).ReturnsAsync(savedJob);

            // Act
            var result = await _jobService.AddJobAsync(dto, recruiterId);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(dto.Title);

            _jobRepoMock.Verify(x => x.AddAsync(It.IsAny<Entities.Job>()), Times.Once);
            _esServiceMock.Verify(x => x.IndexJobAsync(It.IsAny<JobDocument>()), Times.Once);
            _publishMock.Verify(x => x.Publish(It.IsAny<JobPostedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
