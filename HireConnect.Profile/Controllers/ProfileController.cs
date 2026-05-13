using System.Security.Claims;
using HireConnect.Profile.DTOs;
using HireConnect.Profile.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireConnect.Profile.Controllers;

// ══════════════════════════════════════════════════════════════════════════════
//  CANDIDATE PROFILE CONTROLLER  —  /api/candidates
// ══════════════════════════════════════════════════════════════════════════════

[ApiController]
[Route("api/candidates")]
[Authorize]
public class CandidateProfileController(IProfileService profileService) : ControllerBase
{
    // POST /api/candidates
    [HttpPost]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> AddCandidate([FromBody] CreateCandidateProfileRequest request)
    {
        // Ensure the UserId in the request matches the authenticated token
        if (request.UserId != GetUserIdFromToken())
            return Forbid();

        try   { return Ok(await profileService.AddCandidateProfileAsync(request)); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // GET /api/candidates/user/{userId}
    [HttpGet("user/{userId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        try   { return Ok(await profileService.GetCandidateByUserIdAsync(userId)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // GET /api/candidates/me
    [HttpGet("me")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var userId = GetUserIdFromToken();
            return Ok(await profileService.GetCandidateByUserIdAsync(userId));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // GET /api/candidates/{profileId}
    [HttpGet("{profileId:int}")]
    public async Task<IActionResult> GetById(int profileId)
    {
        try   { return Ok(await profileService.GetCandidateByProfileIdAsync(profileId)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }



    // PUT /api/candidates/me
    [HttpPut("me")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateCandidateProfileRequest request)
    {
        try
        {
            var userId = GetUserIdFromToken();
            return Ok(await profileService.UpdateCandidateProfileAsync(userId, request));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { message = ex.Message, detail = ex.InnerException?.Message }); }
    }



    private int GetUserIdFromToken()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");
        return int.Parse(claim);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  RECRUITER PROFILE CONTROLLER  —  /api/recruiters
// ══════════════════════════════════════════════════════════════════════════════

[ApiController]
[Route("api/recruiters")]
[Authorize]
public class RecruiterProfileController(IProfileService profileService) : ControllerBase
{
    // POST /api/recruiters
    [HttpPost]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> AddRecruiter([FromBody] CreateRecruiterProfileRequest request)
    {
        if (request.UserId != GetUserIdFromToken())
            return Forbid();

        try   { return Ok(await profileService.AddRecruiterProfileAsync(request)); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // GET /api/recruiters/user/{userId}
    [HttpGet("user/{userId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        try   { return Ok(await profileService.GetRecruiterByUserIdAsync(userId)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // GET /api/recruiters/me
    [HttpGet("me")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var userId = GetUserIdFromToken();
            return Ok(await profileService.GetRecruiterByUserIdAsync(userId));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // GET /api/recruiters/{profileId}  (any authenticated user — candidates need to see recruiter info)
    [HttpGet("{profileId:int}")]
    public async Task<IActionResult> GetById(int profileId)
    {
        try   { return Ok(await profileService.GetRecruiterByProfileIdAsync(profileId)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }



    // PUT /api/recruiters/me
    [HttpPut("me")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateRecruiterProfileRequest request)
    {
        try
        {
            var userId = GetUserIdFromToken();
            return Ok(await profileService.UpdateRecruiterProfileAsync(userId, request));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }



    private int GetUserIdFromToken()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");
        return int.Parse(claim);
    }
}
