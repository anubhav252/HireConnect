using HireConnect.Profile.Models;

namespace HireConnect.Profile.Repositories;

public interface IProfileRepository
{
    // ── Candidate ─────────────────────────────────────────────────────
    Task<CandidateProfile?>       FindCandidateByEmailAsync(string email);
    Task<CandidateProfile?>       FindCandidateByUserIdAsync(int userId);
    Task<CandidateProfile?>       FindCandidateByProfileIdAsync(int profileId);
    Task<List<CandidateProfile>>  FindAllCandidatesAsync();
    Task<bool>                    CandidateExistsByUserIdAsync(int userId);

    Task<CandidateProfile>        CreateCandidateAsync(CandidateProfile profile);
    Task<CandidateProfile>        UpdateCandidateAsync(CandidateProfile profile);
    Task                          DeleteCandidateByProfileIdAsync(int profileId);

    // ── Recruiter ─────────────────────────────────────────────────────
    Task<RecruiterProfile?>       FindRecruiterByEmailAsync(string email);
    Task<RecruiterProfile?>       FindRecruiterByUserIdAsync(int userId);
    Task<RecruiterProfile?>       FindRecruiterByProfileIdAsync(int profileId);
    Task<List<RecruiterProfile>>  FindAllRecruitersAsync();
    Task<bool>                    RecruiterExistsByUserIdAsync(int userId);

    Task<RecruiterProfile>        CreateRecruiterAsync(RecruiterProfile profile);
    Task<RecruiterProfile>        UpdateRecruiterAsync(RecruiterProfile profile);
    Task                          DeleteRecruiterByProfileIdAsync(int profileId);
}
