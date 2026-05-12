namespace HireConnect.Auth.DTOs;

public record RegisterRequest(
    string Email, 
    string Password, 
    string Role,
    string? FullName = null
    );

public record LoginRequest(
    string Email, 
    string Password
    );

public record OAuthUserInfo(
    string Email, 
    string Name, 
    string Provider
    );

public record AuthResponse(
    string Token,
    string RefreshToken,
    string Email,
    string Role,
    int UserId
);

public record MeResponse(
    int UserId,
    string Email,
    string Role,
    string Provider,
    DateTime CreatedAt
);

public record UpdatePasswordRequest(
    string CurrentPassword, 
    string NewPassword
    );

public record RefreshTokenRequest(
    string RefreshToken
    );

public record DeleteAccountRequest(
    string Password
    );