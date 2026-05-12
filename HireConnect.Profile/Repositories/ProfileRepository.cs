using HireConnect.Profile.Data;
using HireConnect.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Profile.Repositories;

public class ProfileRepository(ProfileDbContext db) : IProfileRepository
{
    // ── Candidate ─────────────────────────────────────────────────────

    public async Task<CandidateProfile?> FindCandidateByEmailAsync(string email)
        => await db.CandidateProfiles
                   .Include(c => c.Addresses)
                   .FirstOrDefaultAsync(c => c.Email == email);

    public async Task<CandidateProfile?> FindCandidateByUserIdAsync(int userId)
        => await db.CandidateProfiles
                   .Include(c => c.Addresses)
                   .FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<CandidateProfile?> FindCandidateByProfileIdAsync(int profileId)
        => await db.CandidateProfiles
                   .Include(c => c.Addresses)
                   .FirstOrDefaultAsync(c => c.ProfileId == profileId);

    public async Task<List<CandidateProfile>> FindAllCandidatesAsync()
        => await db.CandidateProfiles
                   .Include(c => c.Addresses)
                   .ToListAsync();

    public async Task<bool> CandidateExistsByUserIdAsync(int userId)
        => await db.CandidateProfiles.AnyAsync(c => c.UserId == userId);

    public async Task<CandidateProfile> CreateCandidateAsync(CandidateProfile profile)
    {
        db.CandidateProfiles.Add(profile);
        await db.SaveChangesAsync();
        return profile;
    }

    public async Task<CandidateProfile> UpdateCandidateAsync(CandidateProfile profile)
    {
        db.CandidateProfiles.Update(profile);
        await db.SaveChangesAsync();
        return profile;
    }

    public async Task DeleteCandidateByProfileIdAsync(int profileId)
    {
        var profile = await db.CandidateProfiles.FindAsync(profileId);
        if (profile != null)
        {
            db.CandidateProfiles.Remove(profile);
            await db.SaveChangesAsync();
        }
    }

    // ── Recruiter ─────────────────────────────────────────────────────

    public async Task<RecruiterProfile?> FindRecruiterByEmailAsync(string email)
        => await db.RecruiterProfiles
                   .Include(r => r.Addresses)
                   .FirstOrDefaultAsync(r => r.Email == email);

    public async Task<RecruiterProfile?> FindRecruiterByUserIdAsync(int userId)
        => await db.RecruiterProfiles
                   .Include(r => r.Addresses)
                   .FirstOrDefaultAsync(r => r.UserId == userId);

    public async Task<RecruiterProfile?> FindRecruiterByProfileIdAsync(int profileId)
        => await db.RecruiterProfiles
                   .Include(r => r.Addresses)
                   .FirstOrDefaultAsync(r => r.ProfileId == profileId);

    public async Task<List<RecruiterProfile>> FindAllRecruitersAsync()
        => await db.RecruiterProfiles
                   .Include(r => r.Addresses)
                   .ToListAsync();

    public async Task<bool> RecruiterExistsByUserIdAsync(int userId)
        => await db.RecruiterProfiles.AnyAsync(r => r.UserId == userId);

    public async Task<RecruiterProfile> CreateRecruiterAsync(RecruiterProfile profile)
    {
        db.RecruiterProfiles.Add(profile);
        await db.SaveChangesAsync();
        return profile;
    }

    public async Task<RecruiterProfile> UpdateRecruiterAsync(RecruiterProfile profile)
    {
        db.RecruiterProfiles.Update(profile);
        await db.SaveChangesAsync();
        return profile;
    }

    public async Task DeleteRecruiterByProfileIdAsync(int profileId)
    {
        var profile = await db.RecruiterProfiles.FindAsync(profileId);
        if (profile != null)
        {
            db.RecruiterProfiles.Remove(profile);
            await db.SaveChangesAsync();
        }
    }
}
