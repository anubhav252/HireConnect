using HireConnect.Profile.DTOs;
using HireConnect.Profile.Services;
using HireConnect.Shared.Events;
using MassTransit;

namespace HireConnect.Profile.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly IProfileService _profileService;
        private readonly ILogger<UserRegisteredConsumer> _logger;

        public UserRegisteredConsumer(IProfileService profileService, ILogger<UserRegisteredConsumer> logger)
        {
            _profileService = profileService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("User Registered Event Received: {UserId}, {Email}, {Role}", msg.UserId, msg.Email, msg.Role);

            try
            {
                if (msg.Role.Equals("Candidate", StringComparison.OrdinalIgnoreCase))
                {
                    var request = new CreateCandidateProfileRequest(
                        msg.UserId,
                        msg.FullName,
                        msg.Email,
                        "", // Mobile placeholder
                        DateTime.SpecifyKind(new DateTime(1990, 1, 1), DateTimeKind.Utc), // DOB placeholder
                        "Welcome to HireConnect! Complete your candidate profile.",
                        new List<string>(),
                        0,
                        "",
                        new List<AddressRequest>()
                    );
                    await _profileService.AddCandidateProfileAsync(request);
                    _logger.LogInformation("Candidate profile shell created for User {UserId}", msg.UserId);
                }
                else if (msg.Role.Equals("Recruiter", StringComparison.OrdinalIgnoreCase))
                {
                    var request = new CreateRecruiterProfileRequest(
                        msg.UserId,
                        msg.FullName,
                        msg.Email,
                        "", // Mobile placeholder
                        "Company Name", // CompanyName placeholder
                        "Small",
                        "Technology",
                        "https://example.com",
                        "Welcome to HireConnect! Complete your recruiter profile.",
                        "",
                        new List<AddressRequest>()
                    );
                    await _profileService.AddRecruiterProfileAsync(request);
                    _logger.LogInformation("Recruiter profile shell created for User {UserId}", msg.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create profile shell for User {UserId}", msg.UserId);
            }
        }
    }
}
