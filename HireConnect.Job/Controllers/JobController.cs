using HireConnect.Job.DTOs;
using HireConnect.Job.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HireConnect.Job.Controllers
{
    /// <summary>
    /// REST API controller for the Job-Service.
    /// Route: /api/jobs
    ///
    /// Auth rules:
    ///   - POST, PUT, DELETE → [Authorize(Roles = "Recruiter")]
    ///   - GET (browse/search) → [AllowAnonymous] so guests can browse
    /// </summary>
    [ApiController]
    [Route("api/jobs")]
    [Produces("application/json")]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly ILogger<JobController> _logger;

        public JobController(IJobService jobService, ILogger<JobController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }
        
        
        // ── POST /api/jobs ───────────────────────────────────────────────────────
        /// <summary>Create a new job posting. Recruiters only.</summary>
        [HttpPost]
        [Authorize(Roles = "Recruiter")]
        [ProducesResponseType(typeof(ApiResponse<JobResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddJob([FromBody] JobRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid request payload."));

            var recruiterId = GetCurrentUserId();
            var created = await _jobService.AddJobAsync(dto, recruiterId);

            return CreatedAtAction(
                nameof(GetJobById),
                new { id = created.JobId },
                ApiResponse<JobResponseDto>.Ok(created, "Job created successfully."));
        }

        // ── GET /api/jobs ────────────────────────────────────────────────────────

        /// <summary>Get all job postings. Public access.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<JobResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllJobs()
        {
            var jobs = await _jobService.GetAllJobsAsync();
            return Ok(ApiResponse<IEnumerable<JobResponseDto>>.Ok(jobs));
        }

        // ── GET /api/jobs/{id} ───────────────────────────────────────────────────

        /// <summary>Get a specific job by ID. Public access.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<JobResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetJobById([FromRoute] int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            return Ok(ApiResponse<JobResponseDto>.Ok(job));
        }

        // ── GET /api/jobs/search ─────────────────────────────────────────────────

        /// <summary>
        /// Search and filter jobs via Elasticsearch.
        /// Supports: keyword, minSalary, maxSalary, category, location, experience, page, pageSize.
        /// Public access (guests can search without logging in).
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<JobResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchJobs(
            [FromQuery] string? keyword = "",
            [FromQuery] double? minSalary = null,
            [FromQuery] double? maxSalary = null,
            [FromQuery] string? category = null,
            [FromQuery] string? location = null,
            [FromQuery] int? experienceRequired = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var results = await _jobService.SearchJobsAsync(
                keyword ?? string.Empty,
                minSalary, maxSalary,
                category, location,
                experienceRequired,
                page, pageSize);

            return Ok(ApiResponse<IEnumerable<JobResponseDto>>.Ok(results));
        }



        // ── GET /api/jobs/recruiter/{recruiterId} ─────────────────────────────────

        /// <summary>Get all jobs posted by a specific recruiter. Authenticated.</summary>
        [HttpGet("recruiter/{recruiterId:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<JobResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetJobsByRecruiter([FromRoute] int recruiterId)
        {
            var jobs = await _jobService.GetJobsByRecruiterAsync(recruiterId);
            return Ok(ApiResponse<IEnumerable<JobResponseDto>>.Ok(jobs));
        }

        // ── PUT /api/jobs/{id} ────────────────────────────────────────────────────

        /// <summary>Update an existing job. Only the owning recruiter can update.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Recruiter")]
        [ProducesResponseType(typeof(ApiResponse<JobResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateJob([FromRoute] int id,
                                                    [FromBody] JobRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid request payload."));

            var recruiterId = GetCurrentUserId();
            var updated = await _jobService.UpdateJobAsync(id, dto, recruiterId);

            return Ok(ApiResponse<JobResponseDto>.Ok(updated, "Job updated successfully."));
        }

        // ── DELETE /api/jobs/{id} ─────────────────────────────────────────────────

        /// <summary>Delete a job posting. Only the owning recruiter can delete.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Recruiter")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteJob([FromRoute] int id)
        {
            var recruiterId = GetCurrentUserId();
            await _jobService.DeleteJobAsync(id, recruiterId);
            return NoContent();
        }

        // ── Private Helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// Extracts the authenticated user's ID from the JWT "sub" or "nameid" claim.
        /// Throws if claim is missing or malformed.
        /// </summary>
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("sub")
                        ?? throw new UnauthorizedAccessException("User ID claim not found in token.");

            if (!int.TryParse(claim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID claim format.");

            return userId;
        }
    }
}
