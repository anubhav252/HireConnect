using HireConnect.Auth.Data;
using HireConnect.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Auth.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AuthDbContext _db;

    public AuthRepository(AuthDbContext db) => _db = db;

    public async Task<UserCredential?> FindByEmailAsync(string email)
        => await _db.UserCredentials.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<UserCredential?> FindByUserIdAsync(int userId)
        => await _db.UserCredentials.FindAsync(userId);

    public async Task<bool> ExistsByEmailAsync(string email)
        => await _db.UserCredentials.AnyAsync(u => u.Email == email);

    public async Task<UserCredential> CreateAsync(UserCredential credential)
    {
        _db.UserCredentials.Add(credential);
        await _db.SaveChangesAsync();
        return credential;
    }

    public async Task<UserCredential> UpdateAsync(UserCredential credential)
    {
        _db.UserCredentials.Update(credential);
        await _db.SaveChangesAsync();
        return credential;
    }

    public async Task DeleteByUserIdAsync(int userId)
    {
        var user = await _db.UserCredentials.FindAsync(userId);
        if (user != null)
        {
            _db.UserCredentials.Remove(user);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken token)
    {
        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync();
        return token;
    }

    public async Task<RefreshToken?> FindRefreshTokenAsync(string token)
        => await _db.RefreshTokens
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Token == token);

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
        if (rt != null)
        {
            rt.IsRevoked = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task RevokeAllUserRefreshTokensAsync(int userId)
    {
        var tokens = await _db.RefreshTokens
                               .Where(r => r.UserId == userId && !r.IsRevoked)
                               .ToListAsync();
        tokens.ForEach(t => t.IsRevoked = true);
        await _db.SaveChangesAsync();
    }
}