using HireConnect.Profile.DTOs;
using HireConnect.Profile.Models;
using HireConnect.Profile.Repositories;

namespace HireConnect.Profile.Services;

public class ProfileServiceImpl(IProfileRepository repo) : IProfileService
{
    // ═══════════════════════════════════════════════════════════════
    //  CANDIDATE
    // ═══════════════════════════════════════════════════════════════

    public async Task<CandidateProfileResponse> AddCandidateProfileAsync(CreateCandidateProfileRequest request)
    {
        if (await repo.CandidateExistsByUserIdAsync(request.UserId))
            throw new InvalidOperationException("A profile already exists for this user.");

        var profile = new CandidateProfile
        {
            UserId     = request.UserId,
            FullName   = request.FullName.Trim(),
            Email      = request.Email.ToLowerInvariant().Trim(),
            Mobile     = request.Mobile.Trim(),
            Dob        = DateTime.SpecifyKind(request.Dob, DateTimeKind.Utc),
            Bio        = request.Bio.Trim(),
            Experience = request.Experience,
            ResumeUrl  = request.ResumeUrl.Trim(),
            Addresses  = MapAddresses(request.Addresses, "Candidate")
        };
        profile.SetSkills(request.Skills);

        var saved = await repo.CreateCandidateAsync(profile);
        return MapCandidateResponse(saved);
    }

    public async Task<CandidateProfileResponse> GetCandidateByUserIdAsync(int userId)
    {
        var profile = await repo.FindCandidateByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Candidate profile not found.");
        return MapCandidateResponse(profile);
    }

    public async Task<CandidateProfileResponse> GetCandidateByProfileIdAsync(int profileId)
    {
        var profile = await repo.FindCandidateByProfileIdAsync(profileId)
            ?? throw new KeyNotFoundException("Candidate profile not found.");
        return MapCandidateResponse(profile);
    }

    public async Task<List<CandidateProfileResponse>> GetAllCandidatesAsync()
    {
        var profiles = await repo.FindAllCandidatesAsync();
        return profiles.Select(MapCandidateResponse).ToList();
    }

    public async Task<CandidateProfileResponse> UpdateCandidateProfileAsync(int userId, UpdateCandidateProfileRequest request)
    {
        var profile = await repo.FindCandidateByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Candidate profile not found.");

        if (request.FullName   is not null) profile.FullName   = request.FullName.Trim();
        if (request.Mobile     is not null) profile.Mobile     = request.Mobile.Trim();
        if (request.Dob        is not null) profile.Dob        = DateTime.SpecifyKind(request.Dob.Value, DateTimeKind.Utc);
        if (request.Bio        is not null) profile.Bio        = request.Bio.Trim();
        if (request.Experience is not null) profile.Experience = request.Experience.Value;
        if (request.ResumeUrl  is not null) profile.ResumeUrl  = request.ResumeUrl.Trim();
        if (request.Skills     is not null) profile.SetSkills(request.Skills);

        if (request.Addresses is not null)
        {
            profile.Addresses.Clear();
            profile.Addresses.AddRange(MapAddresses(request.Addresses, "Candidate", profile.ProfileId));
        }

        profile.UpdatedAt = DateTime.UtcNow;
        var updated = await repo.UpdateCandidateAsync(profile);
        return MapCandidateResponse(updated);
    }

    public async Task DeleteCandidateProfileAsync(int userId)
    {
        var profile = await repo.FindCandidateByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Candidate profile not found.");
        await repo.DeleteCandidateByProfileIdAsync(profile.ProfileId);
    }

    // ═══════════════════════════════════════════════════════════════
    //  RECRUITER
    // ═══════════════════════════════════════════════════════════════

    public async Task<RecruiterProfileResponse> AddRecruiterProfileAsync(CreateRecruiterProfileRequest request)
    {
        if (await repo.RecruiterExistsByUserIdAsync(request.UserId))
            throw new InvalidOperationException("A profile already exists for this user.");

        var profile = new RecruiterProfile
        {
            UserId      = request.UserId,
            FullName    = request.FullName.Trim(),
            Email       = request.Email.ToLowerInvariant().Trim(),
            Mobile      = request.Mobile.Trim(),
            CompanyName = request.CompanyName.Trim(),
            CompanySize = request.CompanySize.Trim(),
            Industry    = request.Industry.Trim(),
            Website     = request.Website.Trim(),
            Bio         = request.Bio.Trim(),
            LogoUrl     = request.LogoUrl.Trim(),
            Addresses   = MapAddresses(request.Addresses, "Recruiter")
        };

        var saved = await repo.CreateRecruiterAsync(profile);
        return MapRecruiterResponse(saved);
    }

    public async Task<RecruiterProfileResponse> GetRecruiterByUserIdAsync(int userId)
    {
        var profile = await repo.FindRecruiterByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Recruiter profile not found.");
        return MapRecruiterResponse(profile);
    }

    public async Task<RecruiterProfileResponse> GetRecruiterByProfileIdAsync(int profileId)
    {
        var profile = await repo.FindRecruiterByProfileIdAsync(profileId)
            ?? throw new KeyNotFoundException("Recruiter profile not found.");
        return MapRecruiterResponse(profile);
    }

    public async Task<List<RecruiterProfileResponse>> GetAllRecruitersAsync()
    {
        var profiles = await repo.FindAllRecruitersAsync();
        return profiles.Select(MapRecruiterResponse).ToList();
    }

    public async Task<RecruiterProfileResponse> UpdateRecruiterProfileAsync(int userId, UpdateRecruiterProfileRequest request)
    {
        var profile = await repo.FindRecruiterByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Recruiter profile not found.");

        if (request.FullName    is not null) profile.FullName    = request.FullName.Trim();
        if (request.Mobile      is not null) profile.Mobile      = request.Mobile.Trim();
        if (request.CompanyName is not null) profile.CompanyName = request.CompanyName.Trim();
        if (request.CompanySize is not null) profile.CompanySize = request.CompanySize.Trim();
        if (request.Industry    is not null) profile.Industry    = request.Industry.Trim();
        if (request.Website     is not null) profile.Website     = request.Website.Trim();
        if (request.Bio         is not null) profile.Bio         = request.Bio.Trim();
        if (request.LogoUrl     is not null) profile.LogoUrl     = request.LogoUrl.Trim();

        if (request.Addresses is not null)
        {
            profile.Addresses.Clear();
            profile.Addresses.AddRange(MapAddresses(request.Addresses, "Recruiter", profile.ProfileId));
        }

        profile.UpdatedAt = DateTime.UtcNow;
        var updated = await repo.UpdateRecruiterAsync(profile);
        return MapRecruiterResponse(updated);
    }

    public async Task DeleteRecruiterProfileAsync(int userId)
    {
        var profile = await repo.FindRecruiterByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Recruiter profile not found.");
        await repo.DeleteRecruiterByProfileIdAsync(profile.ProfileId);
    }

    // ═══════════════════════════════════════════════════════════════
    //  PRIVATE MAPPING HELPERS
    // ═══════════════════════════════════════════════════════════════

    private static List<Address> MapAddresses(
        List<AddressRequest> requests,
        string profileType,
        int profileId = 0)
        => requests.Select(a => new Address
        {
            HouseNo     = a.HouseNo.Trim(),
            Street      = a.Street.Trim(),
            City        = a.City.Trim(),
            State       = a.State.Trim(),
            Pincode     = a.Pincode.Trim(),
            ProfileId   = profileId,
            ProfileType = profileType
        }).ToList();

    private static List<AddressDto> MapAddressDtos(List<Address> addresses)
        => addresses.Select(a => new AddressDto(
            a.AddressId, a.HouseNo, a.Street, a.City, a.State, a.Pincode
        )).ToList();

    private static CandidateProfileResponse MapCandidateResponse(CandidateProfile p)
        => new(
            p.ProfileId, p.UserId, p.FullName, p.Email, p.Mobile,
            p.Dob, p.Bio, p.GetSkills(), p.Experience, p.ResumeUrl,
            p.CreatedAt, p.UpdatedAt, MapAddressDtos(p.Addresses)
        );

    private static RecruiterProfileResponse MapRecruiterResponse(RecruiterProfile p)
        => new(
            p.ProfileId, p.UserId, p.FullName, p.Email, p.Mobile,
            p.CompanyName, p.CompanySize, p.Industry, p.Website,
            p.Bio, p.LogoUrl, p.CreatedAt, p.UpdatedAt,
            MapAddressDtos(p.Addresses)
        );
}
