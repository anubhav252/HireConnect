using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HireConnect.Auth.DTOs;
using HireConnect.Auth.Models;
using HireConnect.Auth.Repositories;
using HireConnect.Shared.Events;
using MassTransit;
using Microsoft.IdentityModel.Tokens;

namespace HireConnect.Auth.Services;

public class AuthServiceImpl : IAuthService
{
    private readonly IAuthRepository _repo;
    private readonly IConfiguration  _config;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuthServiceImpl> _logger;

    public AuthServiceImpl(IAuthRepository repo, IConfiguration config, IPublishEndpoint publishEndpoint, ILogger<AuthServiceImpl> logger)
    {
        _repo   = repo;
        _config = config;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _repo.ExistsByEmailAsync(request.Email))
            throw new InvalidOperationException("Email already registered.");

        if (request.Role != "Candidate" && request.Role != "Recruiter")
            throw new ArgumentException("Role must be 'Candidate' or 'Recruiter'.");

        var credential = new UserCredential
        {
            Email        = request.Email.ToLowerInvariant().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role         = request.Role,
            Provider     = "Local"
        };

        var saved = await _repo.CreateAsync(credential);

        _logger.LogInformation("Publishing UserRegisteredEvent for User {UserId}...", saved.UserId);
        await _publishEndpoint.Publish(new UserRegisteredEvent(
            saved.UserId,
            saved.Email,
            request.FullName ?? saved.Email,
            saved.Role
        ));
        _logger.LogInformation("UserRegisteredEvent published successfully.");

        return await BuildAuthResponseAsync(saved);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _repo.FindByEmailAsync(request.Email.ToLowerInvariant().Trim())
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.Provider != "Local")
            throw new InvalidOperationException(
                $"This account uses {user.Provider} login. Please sign in with {user.Provider}.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> HandleOAuthLoginAsync(OAuthUserInfo oauthUser, string role)
    {
        var existing = await _repo.FindByEmailAsync(oauthUser.Email.ToLowerInvariant().Trim());

        if (existing != null)
            return await BuildAuthResponseAsync(existing);

        var newUser = new UserCredential
        {
            Email        = oauthUser.Email.ToLowerInvariant().Trim(),
            PasswordHash = string.Empty,
            Role         = role,
            Provider     = oauthUser.Provider
        };

        var saved = await _repo.CreateAsync(newUser);

        await _publishEndpoint.Publish(new UserRegisteredEvent(
            saved.UserId,
            saved.Email,
            oauthUser.Name ?? saved.Email,
            saved.Role
        ));

        return await BuildAuthResponseAsync(saved);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var stored = await _repo.FindRefreshTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!stored.IsActive)
            throw new UnauthorizedAccessException("Refresh token has expired or been revoked.");

        await _repo.RevokeRefreshTokenAsync(refreshToken);
        return await BuildAuthResponseAsync(stored.User);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
            new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(key),
                ValidateIssuer           = true,
                ValidIssuer              = _config["Jwt:Issuer"],
                ValidateAudience         = true,
                ValidAudience            = _config["Jwt:Audience"],
                ClockSkew                = TimeSpan.Zero
            }, out _);
            return true;
        }
        catch { return false; }
    }

    public async Task<MeResponse> GetMeAsync(int userId)
    {
        var user = await _repo.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return new MeResponse(user.UserId, user.Email, user.Role, user.Provider, user.CreatedAt);
    }

    public async Task LogoutAsync(int userId, string refreshToken)
    {
        await _repo.RevokeRefreshTokenAsync(refreshToken);
    }

    public async Task UpdatePasswordAsync(int userId, UpdatePasswordRequest request)
    {
        var user = await _repo.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Provider != "Local")
            throw new InvalidOperationException("OAuth accounts do not have a password.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        if (request.NewPassword.Length < 8)
            throw new ArgumentException("New password must be at least 8 characters.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _repo.UpdateAsync(user);
        await _repo.RevokeAllUserRefreshTokensAsync(userId);
    }

    public async Task DeleteAccountAsync(int userId, DeleteAccountRequest request)
    {
        var user = await _repo.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Provider == "Local")
        {
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Password is incorrect.");
        }
        else
        {
            if (request.Password != "DELETE MY ACCOUNT")
                throw new UnauthorizedAccessException("Type 'DELETE MY ACCOUNT' to confirm.");
        }

        await _repo.DeleteByUserIdAsync(userId);
    }

    // ── Private helpers ──────────────────────────────────────────────

    private async Task<AuthResponse> BuildAuthResponseAsync(UserCredential user)
    {
        var jwt = GenerateJwt(user);
        var rt  = await PersistRefreshTokenAsync(user.UserId);
        return new AuthResponse(jwt, rt, user.Email, user.Role, user.UserId);
    }

    private async Task<string> PersistRefreshTokenAsync(int userId)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        await _repo.SaveRefreshTokenAsync(new RefreshToken
        {
            UserId    = userId,
            Token     = raw,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        return raw;
    }

    private string GenerateJwt(UserCredential user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email,           user.Email),
            new Claim(ClaimTypes.Role,            user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}