using FluentAssertions;
using HireConnect.Auth.DTOs;
using HireConnect.Auth.Models;
using HireConnect.Auth.Repositories;
using HireConnect.Auth.Services;
using HireConnect.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace HireConnect.Auth.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IAuthRepository> _repoMock;
        private Mock<IConfiguration> _configMock;
        private Mock<IPublishEndpoint> _publishMock;
        private Mock<ILogger<AuthServiceImpl>> _loggerMock;
        private AuthServiceImpl _authService;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IAuthRepository>();
            _configMock = new Mock<IConfiguration>();
            _publishMock = new Mock<IPublishEndpoint>();
            _loggerMock = new Mock<ILogger<AuthServiceImpl>>();

            // Default config values
            _configMock.Setup(x => x["Jwt:Key"]).Returns("Test_Secret_Key_At_Least_32_Chars_Long!");
            _configMock.Setup(x => x["Jwt:Issuer"]).Returns("HireConnect");
            _configMock.Setup(x => x["Jwt:Audience"]).Returns("HireConnect.Users");

            _authService = new AuthServiceImpl(_repoMock.Object, _configMock.Object, _publishMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task RegisterAsync_WhenUserDoesNotExist_ShouldCreateUserAndPublishEvent()
        {
            // Arrange
            // Order: Email, Password, Role, FullName
            var request = new RegisterRequest("test@example.com", "password123", "Candidate", "Test User");
            _repoMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
            _repoMock.Setup(x => x.CreateAsync(It.IsAny<UserCredential>())).ReturnsAsync(new UserCredential { UserId = 1, Email = request.Email, Role = "Candidate" });

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(request.Email);
            
            _repoMock.Verify(x => x.CreateAsync(It.IsAny<UserCredential>()), Times.Once);
            _publishMock.Verify(x => x.Publish(It.IsAny<UserRegisteredEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void RegisterAsync_WhenUserAlreadyExists_ShouldThrowException()
        {
            // Arrange
            var request = new RegisterRequest("existing@example.com", "password123", "Candidate", "Existing User");
            _repoMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _authService.RegisterAsync(request);

            // Assert
            act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Email already registered.");
        }
    }
}
