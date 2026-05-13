using System.Security.Claims;
using HireConnect.Auth.DTOs;
using HireConnect.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace HireConnect.Auth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    // ── Public ───────────────────────────────────────────────────────

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try   { return Ok(await _authService.RegisterAsync(request)); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try   { return Ok(await _authService.LoginAsync(request)); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
        catch (InvalidOperationException ex)   { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try   { return Ok(await _authService.RefreshTokenAsync(request.RefreshToken)); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] string token)
        => Ok(new { isValid = await _authService.ValidateTokenAsync(token) });

    // ── Protected ────────────────────────────────────────────────────

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        try
        {
            var userId = GetUserIdFromToken();
            return Ok(await _authService.GetMeAsync(userId));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var userId = GetUserIdFromToken();
        await _authService.LogoutAsync(userId, request.RefreshToken);
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpPatch("update-password")]
    [Authorize]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        try
        {
            var userId = GetUserIdFromToken();
            await _authService.UpdatePasswordAsync(userId, request);
            return Ok(new { message = "Password updated. Please log in again." });
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
        catch (InvalidOperationException ex)   { return BadRequest(new { message = ex.Message }); }
        catch (ArgumentException ex)           { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("delete-account")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        try
        {
            var userId = GetUserIdFromToken();
            await _authService.DeleteAccountAsync(userId, request);
            return Ok(new { message = "Account permanently deleted." });
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
        catch (KeyNotFoundException ex)        { return NotFound(new { message = ex.Message }); }
    }

    // ── Helper ───────────────────────────────────────────────────────

    private int GetUserIdFromToken()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");
        return int.Parse(claim);
    }

    [HttpGet("login/github")]
    public IActionResult LoginWithGitHub([FromQuery] string role = "Candidate")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GitHubCallback), new { role })
        };
        return Challenge(properties, "GitHub");
    }

    [HttpGet("oauth-success/github")]
    public async Task<IActionResult> GitHubCallback([FromQuery] string role = "Candidate")
    {
        var result = await HttpContext.AuthenticateAsync("GitHub");

        if (!result.Succeeded)
            return Unauthorized(new { message = "GitHub authentication failed." });

        var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
        var name  = result.Principal?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "GitHub did not return an email. Make your GitHub email public." });

        var response = await _authService.HandleOAuthLoginAsync(
            new OAuthUserInfo(email, name, "GitHub"), role);

        // Redirect back to Angular frontend with token
        var frontendUrl = $"http://localhost:4200/auth/callback?token={response.Token}&userId={response.UserId}&email={response.Email}&role={response.Role}";
        return Redirect(frontendUrl);
    }

    [HttpGet("login/google")]
    public IActionResult LoginWithGoogle([FromQuery] string role = "Candidate")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback), new { role })
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("oauth-success/google")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string role = "Candidate")
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded)
            return Unauthorized(new { message = "Google authentication failed." });

        var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
        var name  = result.Principal?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "Google did not return an email." });

        var response = await _authService.HandleOAuthLoginAsync(
            new OAuthUserInfo(email, name, "Google"), role);

        // Redirect back to Angular frontend with token
        var frontendUrl = $"http://localhost:4200/auth/callback?token={response.Token}&userId={response.UserId}&email={response.Email}&role={response.Role}";
        return Redirect(frontendUrl);
    }
}