using HireConnect.Auth.DTOs;
using HireConnect.Auth.Models;

namespace HireConnect.Auth.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> HandleOAuthLoginAsync(OAuthUserInfo oauthUser, string role);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);

    Task<MeResponse> GetMeAsync(int userId);
    Task LogoutAsync(int userId, string refreshToken);
    Task UpdatePasswordAsync(int userId, UpdatePasswordRequest request);
    Task DeleteAccountAsync(int userId, DeleteAccountRequest request);
}