using HireConnect.Auth.Models;

namespace HireConnect.Auth.Repositories;

public interface IAuthRepository
{
    Task<UserCredential?> FindByEmailAsync(string email);
    Task<UserCredential?> FindByUserIdAsync(int userId);
    Task<bool> ExistsByEmailAsync(string email);
    Task<UserCredential> CreateAsync(UserCredential credential);
    Task<UserCredential> UpdateAsync(UserCredential credential);
    Task DeleteByUserIdAsync(int userId);

    Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken token);
    Task<RefreshToken?> FindRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    Task RevokeAllUserRefreshTokensAsync(int userId);
}