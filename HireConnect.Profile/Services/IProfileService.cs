using HireConnect.Profile.DTOs;

namespace HireConnect.Profile.Services;

public interface IProfileService
{
    // ── Candidate ─────────────────────────────────────────────────────
    Task<CandidateProfileResponse>  AddCandidateProfileAsync(CreateCandidateProfileRequest request);
    Task<CandidateProfileResponse>  GetCandidateByUserIdAsync(int userId);
    Task<CandidateProfileResponse>  GetCandidateByProfileIdAsync(int profileId);
    Task<List<CandidateProfileResponse>> GetAllCandidatesAsync();
    Task<CandidateProfileResponse>  UpdateCandidateProfileAsync(int userId, UpdateCandidateProfileRequest request);
    Task                            DeleteCandidateProfileAsync(int userId);

    // ── Recruiter ─────────────────────────────────────────────────────
    Task<RecruiterProfileResponse>  AddRecruiterProfileAsync(CreateRecruiterProfileRequest request);
    Task<RecruiterProfileResponse>  GetRecruiterByUserIdAsync(int userId);
    Task<RecruiterProfileResponse>  GetRecruiterByProfileIdAsync(int profileId);
    Task<List<RecruiterProfileResponse>> GetAllRecruitersAsync();
    Task<RecruiterProfileResponse>  UpdateRecruiterProfileAsync(int userId, UpdateRecruiterProfileRequest request);
    Task                            DeleteRecruiterProfileAsync(int userId);
}
