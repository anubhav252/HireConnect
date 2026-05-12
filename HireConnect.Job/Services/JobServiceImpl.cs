using HireConnect.Job.DTOs;
using HireConnect.Job.Elasticsearch;
using HireConnect.Job.Middleware;
using HireConnect.Job.Repositories;
using HireConnect.Shared.Events;
using MassTransit;

namespace HireConnect.Job.Services
{
    /// <summary>
    /// Core business logic implementation for the Job-Service.
    /// Orchestrates:
    ///   - IJobRepository (EF Core / PostgreSQL)
    ///   - IJobElasticsearchService (full-text search + sync)
    /// </summary>
    public class JobServiceImpl : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IJobElasticsearchService _elasticsearchService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<JobServiceImpl> _logger;

        // Valid status values
        private static readonly HashSet<string> ValidStatuses =
            new(StringComparer.OrdinalIgnoreCase) { "Active", "Paused", "Closed" };

        // Valid job types
        private static readonly HashSet<string> ValidTypes =
            new(StringComparer.OrdinalIgnoreCase)
            { "Full-Time", "Part-Time", "Contract", "Internship", "Remote" };

        public JobServiceImpl(IJobRepository jobRepository,
                              IJobElasticsearchService elasticsearchService,
                              IPublishEndpoint publishEndpoint,
                              ILogger<JobServiceImpl> logger)
        {
            _jobRepository = jobRepository;
            _elasticsearchService = elasticsearchService;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        // ── CRUD ────────────────────────────────────────────────────────────────

        public async Task<JobResponseDto> AddJobAsync(JobRequestDto dto, int recruiterId)
        {
            ValidateJobRequest(dto);

            var job = MapToEntity(dto);
            job.PostedBy = recruiterId; // always set from JWT claim, not DTO body
            job.PostedAt = DateTime.UtcNow;

            var created = await _jobRepository.AddAsync(job);

            // Sync to Elasticsearch
            await SyncToElasticsearchSafeAsync(created, isDelete: false);

            // Publish Event
            await _publishEndpoint.Publish(new JobPostedEvent(
                created.JobId,
                created.PostedBy,
                created.Title,
                "HireConnect Employer", // Would ideally fetch company name from Profile service
                created.Location,
                created.Category,
                created.PostedAt
            ));

            return MapToDto(created);
        }

        public async Task<IEnumerable<JobResponseDto>> GetAllJobsAsync()
        {
            var jobs = await _jobRepository.GetAllAsync();
            return jobs.Select(MapToDto);
        }

        public async Task<JobResponseDto> GetJobByIdAsync(int id)
        {
            var job = await _jobRepository.GetByIdAsync(id)
                      ?? throw new NotFoundException($"Job with ID {id} was not found.");

            return MapToDto(job);
        }

        public async Task<JobResponseDto> UpdateJobAsync(int id, JobRequestDto dto, int recruiterId)
        {
            var existing = await _jobRepository.GetByIdAsync(id)
                           ?? throw new NotFoundException($"Job with ID {id} was not found.");

            // Only the recruiter who posted the job can update it
            if (existing.PostedBy != recruiterId)
                throw new ForbiddenException("You are not authorised to update this job.");

            ValidateJobRequest(dto);

            // Apply updates
            existing.Title = dto.Title;
            existing.Category = dto.Category;
            existing.Type = dto.Type;
            existing.Location = dto.Location;
            existing.SalaryMin = dto.SalaryMin;
            existing.SalaryMax = dto.SalaryMax;
            existing.Skills = dto.Skills;
            existing.ExperienceRequired = dto.ExperienceRequired;
            existing.Status = dto.Status;
            existing.Description = dto.Description;
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _jobRepository.UpdateAsync(existing);

            await SyncToElasticsearchSafeAsync(updated, isDelete: false);

            return MapToDto(updated);
        }

        public async Task DeleteJobAsync(int id, int recruiterId)
        {
            var existing = await _jobRepository.GetByIdAsync(id)
                           ?? throw new NotFoundException($"Job with ID {id} was not found.");

            if (existing.PostedBy != recruiterId)
                throw new ForbiddenException("You are not authorised to delete this job.");

            await _jobRepository.DeleteAsync(id);

            // Remove from Elasticsearch
            await SyncToElasticsearchSafeAsync(existing, isDelete: true);
        }

        // ── Search & Filter ─────────────────────────────────────────────────────

        public async Task<IEnumerable<JobResponseDto>> SearchJobsAsync(
            string keyword,
            double? minSalary = null,
            double? maxSalary = null,
            string? category = null,
            string? location = null,
            int? experienceRequired = null,
            int page = 1,
            int pageSize = 20)
        {
            // Normalise pagination
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var from = (page - 1) * pageSize;

            try
            {
                // Primary: Elasticsearch full-text search
                var documents = await _elasticsearchService.SearchAsync(
                    keyword, minSalary, maxSalary, category, location,
                    experienceRequired, from, pageSize);

                return documents.Select(MapDocumentToDto);
            }
            catch (ElasticsearchException esEx)
            {
                // Fallback: DB-level keyword search when ES is unavailable
                _logger.LogWarning(esEx,
                    "Elasticsearch unavailable — falling back to database search for keyword '{Keyword}'",
                    keyword);

                return await DatabaseFallbackSearchAsync(
                    keyword, minSalary, maxSalary, category, location,
                    experienceRequired, from, pageSize);
            }
        }

        public async Task<IEnumerable<JobResponseDto>> GetJobsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new JobValidationException("Category cannot be empty.");

            var jobs = await _jobRepository.FindByCategoryAsync(category);
            return jobs.Select(MapToDto);
        }

        public async Task<IEnumerable<JobResponseDto>> GetJobsByLocationAsync(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                throw new JobValidationException("Location cannot be empty.");

            var jobs = await _jobRepository.FindByLocationAsync(location);
            return jobs.Select(MapToDto);
        }

        public async Task<IEnumerable<JobResponseDto>> GetJobsByRecruiterAsync(int recruiterId)
        {
            if (recruiterId <= 0)
                throw new JobValidationException("Invalid recruiter ID.");

            var jobs = await _jobRepository.FindByPostedByAsync(recruiterId);
            return jobs.Select(MapToDto);
        }

        // ── Private Helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Business-rule validation for a job request DTO.
        /// Throws JobValidationException with all errors collected.
        /// </summary>
        private static void ValidateJobRequest(JobRequestDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Title))
                errors.Add("Title is required.");

            if (string.IsNullOrWhiteSpace(dto.Category))
                errors.Add("Category is required.");

            if (string.IsNullOrWhiteSpace(dto.Location))
                errors.Add("Location is required.");

            if (!ValidTypes.Contains(dto.Type))
                errors.Add($"Type must be one of: {string.Join(", ", ValidTypes)}.");

            if (!ValidStatuses.Contains(dto.Status))
                errors.Add($"Status must be one of: {string.Join(", ", ValidStatuses)}.");

            if (dto.SalaryMin < 0)
                errors.Add("SalaryMin cannot be negative.");

            if (dto.SalaryMax < 0)
                errors.Add("SalaryMax cannot be negative.");

            if (dto.SalaryMax > 0 && dto.SalaryMax < dto.SalaryMin)
                errors.Add("SalaryMax must be greater than or equal to SalaryMin.");

            if (dto.ExperienceRequired < 0 || dto.ExperienceRequired > 50)
                errors.Add("ExperienceRequired must be between 0 and 50.");

            if (errors.Count > 0)
                throw new JobValidationException(errors);
        }

        /// <summary>Non-throwing Elasticsearch sync — logs and continues on failure.</summary>
        private async Task SyncToElasticsearchSafeAsync(Entities.Job job, bool isDelete)
        {
            try
            {
                if (isDelete)
                {
                    await _elasticsearchService.DeleteJobAsync(job.JobId);
                }
                else
                {
                    var doc = new JobDocument
                    {
                        JobId = job.JobId,
                        Title = job.Title,
                        Category = job.Category,
                        Type = job.Type,
                        Location = job.Location,
                        SalaryMin = job.SalaryMin,
                        SalaryMax = job.SalaryMax,
                        Skills = job.Skills,
                        ExperienceRequired = job.ExperienceRequired,
                        PostedBy = job.PostedBy,
                        Status = job.Status,
                        PostedAt = job.PostedAt,
                        Description = job.Description
                    };
                    await _elasticsearchService.IndexJobAsync(doc);
                }
            }
            catch (Exception ex)
            {
                // Elasticsearch sync failure is not fatal — DB is the source of truth
                _logger.LogWarning(ex,
                    "Elasticsearch sync failed for job {JobId}. Will be inconsistent until next re-index.",
                    job.JobId);
            }
        }

        /// <summary>Database-level fallback search when Elasticsearch is unavailable.</summary>
        private async Task<IEnumerable<JobResponseDto>> DatabaseFallbackSearchAsync(
            string keyword,
            double? minSalary,
            double? maxSalary,
            string? category,
            string? location,
            int? experienceRequired,
            int from,
            int pageSize)
        {
            IEnumerable<Entities.Job> jobs;

            if (!string.IsNullOrWhiteSpace(keyword))
                jobs = await _jobRepository.FindByTitleContainingAsync(keyword);
            else
                jobs = await _jobRepository.GetAllAsync();

            // Apply in-memory filters
            if (!string.IsNullOrWhiteSpace(category))
                jobs = jobs.Where(j => j.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(location))
                jobs = jobs.Where(j => j.Location.Contains(location, StringComparison.OrdinalIgnoreCase));

            if (minSalary.HasValue)
                jobs = jobs.Where(j => j.SalaryMin >= minSalary.Value);

            if (maxSalary.HasValue)
                jobs = jobs.Where(j => j.SalaryMax <= maxSalary.Value);

            if (experienceRequired.HasValue)
                jobs = jobs.Where(j => j.ExperienceRequired <= experienceRequired.Value);

            // Only Active jobs in fallback
            jobs = jobs.Where(j => j.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));

            return jobs.Skip(from).Take(pageSize).Select(MapToDto);
        }

        // ── Mappers ─────────────────────────────────────────────────────────────

        private static Entities.Job MapToEntity(JobRequestDto dto) => new()
        {
            Title = dto.Title.Trim(),
            Category = dto.Category.Trim(),
            Type = dto.Type.Trim(),
            Location = dto.Location.Trim(),
            SalaryMin = dto.SalaryMin,
            SalaryMax = dto.SalaryMax,
            Skills = dto.Skills ?? new List<string>(),
            ExperienceRequired = dto.ExperienceRequired,
            Status = dto.Status,
            Description = dto.Description?.Trim()
        };

        private static JobResponseDto MapToDto(Entities.Job job) => new()
        {
            JobId = job.JobId,
            Title = job.Title,
            Category = job.Category,
            Type = job.Type,
            Location = job.Location,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            Skills = job.Skills,
            ExperienceRequired = job.ExperienceRequired,
            PostedBy = job.PostedBy,
            Status = job.Status,
            PostedAt = job.PostedAt,
            UpdatedAt = job.UpdatedAt,
            Description = job.Description
        };

        private static JobResponseDto MapDocumentToDto(JobDocument doc) => new()
        {
            JobId = doc.JobId,
            Title = doc.Title,
            Category = doc.Category,
            Type = doc.Type,
            Location = doc.Location,
            SalaryMin = doc.SalaryMin,
            SalaryMax = doc.SalaryMax,
            Skills = doc.Skills,
            ExperienceRequired = doc.ExperienceRequired,
            PostedBy = doc.PostedBy,
            Status = doc.Status,
            PostedAt = doc.PostedAt,
            Description = doc.Description
        };
    }
}
