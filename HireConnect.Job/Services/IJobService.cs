using HireConnect.Job.DTOs;

namespace HireConnect.Job.Services
{
    /// <summary>
    /// Service interface declaring all business operations for the Job-Service.
    /// Implementations orchestrate repository access + Elasticsearch sync.
    /// </summary>
    public interface IJobService
    {
        // ── CRUD ────────────────────────────────────────────────────────────────

        /// <summary>Create a new job posting and index it in Elasticsearch.</summary>
        Task<JobResponseDto> AddJobAsync(JobRequestDto dto, int recruiterId);

        /// <summary>Return all job postings (ordered by PostedAt desc).</summary>
        Task<IEnumerable<JobResponseDto>> GetAllJobsAsync();

        /// <summary>Return a single job by its primary key.</summary>
        Task<JobResponseDto> GetJobByIdAsync(int id);

        /// <summary>Update an existing job and re-index in Elasticsearch.</summary>
        Task<JobResponseDto> UpdateJobAsync(int id, JobRequestDto dto, int recruiterId);

        /// <summary>Delete a job and remove it from the Elasticsearch index.</summary>
        Task DeleteJobAsync(int id, int recruiterId);

        // ── Search & Filter ─────────────────────────────────────────────────────

        /// <summary>
        /// Full-text search via Elasticsearch with optional salary range filter.
        /// Falls back to DB query if Elasticsearch is unavailable.
        /// </summary>
        Task<IEnumerable<JobResponseDto>> SearchJobsAsync(
            string keyword,
            double? minSalary = null,
            double? maxSalary = null,
            string? category = null,
            string? location = null,
            int? experienceRequired = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>Return all jobs in a specific category.</summary>
        Task<IEnumerable<JobResponseDto>> GetJobsByCategoryAsync(string category);

        /// <summary>Return all jobs matching a location (partial).</summary>
        Task<IEnumerable<JobResponseDto>> GetJobsByLocationAsync(string location);

        /// <summary>Return all jobs posted by a specific recruiter.</summary>
        Task<IEnumerable<JobResponseDto>> GetJobsByRecruiterAsync(int recruiterId);
    }
}
