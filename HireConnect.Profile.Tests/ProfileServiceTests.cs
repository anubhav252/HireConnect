using FluentAssertions;
using HireConnect.Profile.DTOs;
using HireConnect.Profile.Models;
using HireConnect.Profile.Repositories;
using HireConnect.Profile.Services;
using Moq;
using NUnit.Framework;

namespace HireConnect.Profile.Tests
{
    [TestFixture]
    public class ProfileServiceTests
    {
        private Mock<IProfileRepository> _repoMock;
        private ProfileServiceImpl _profileService;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProfileRepository>();
            _profileService = new ProfileServiceImpl(_repoMock.Object);
        }

        [Test]
        public async Task AddCandidateProfileAsync_WhenProfileDoesNotExist_ShouldCreateProfile()
        {
            // Arrange
            var request = new CreateCandidateProfileRequest(
                UserId: 1,
                FullName: "John Doe",
                Email: "john@example.com",
                Mobile: "1234567890",
                Dob: DateTime.UtcNow.AddYears(-25),
                Bio: "Developer",
                Skills: new List<string> { "C#", "SQL" },
                Experience: 3,
                ResumeUrl: "http://resume.com",
                Addresses: new List<AddressRequest>()
            );

            _repoMock.Setup(x => x.CandidateExistsByUserIdAsync(request.UserId)).ReturnsAsync(false);
            _repoMock.Setup(x => x.CreateCandidateAsync(It.IsAny<CandidateProfile>()))
                .ReturnsAsync(new CandidateProfile { ProfileId = 1, UserId = request.UserId, FullName = request.FullName, Email = request.Email });

            // Act
            var result = await _profileService.AddCandidateProfileAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be(request.FullName);
            _repoMock.Verify(x => x.CreateCandidateAsync(It.IsAny<CandidateProfile>()), Times.Once);
        }

        [Test]
        public async Task AddRecruiterProfileAsync_WhenProfileDoesNotExist_ShouldCreateProfile()
        {
            // Arrange
            var request = new CreateRecruiterProfileRequest(
                UserId: 2,
                FullName: "Jane Recruiter",
                Email: "jane@company.com",
                Mobile: "0987654321",
                CompanyName: "Tech Corp",
                CompanySize: "Large",
                Industry: "IT",
                Website: "http://techcorp.com",
                Bio: "HR Manager",
                LogoUrl: "http://logo.com",
                Addresses: new List<AddressRequest>()
            );

            _repoMock.Setup(x => x.RecruiterExistsByUserIdAsync(request.UserId)).ReturnsAsync(false);
            _repoMock.Setup(x => x.CreateRecruiterAsync(It.IsAny<RecruiterProfile>()))
                .ReturnsAsync(new RecruiterProfile { ProfileId = 2, UserId = request.UserId, CompanyName = request.CompanyName, Email = request.Email });

            // Act
            var result = await _profileService.AddRecruiterProfileAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.CompanyName.Should().Be(request.CompanyName);
            _repoMock.Verify(x => x.CreateRecruiterAsync(It.IsAny<RecruiterProfile>()), Times.Once);
        }
    }
}
