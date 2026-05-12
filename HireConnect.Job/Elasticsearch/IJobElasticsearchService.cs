namespace HireConnect.Job.Elasticsearch
{
    /// <summary>
    /// Elasticsearch document model for a Job.
    /// This is the indexed representation — optimised for full-text search.
    /// Index name: "jobs"
    /// </summary>
    public class JobDocument
    {
        public int JobId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public double SalaryMin { get; set; }
        public double SalaryMax { get; set; }
        public List<string> Skills { get; set; } = new();
        public int ExperienceRequired { get; set; }
        public int PostedBy { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Contract for Elasticsearch operations on the "jobs" index.
    /// </summary>
    public interface IJobElasticsearchService
    {
        /// <summary>Index or re-index a single job document.</summary>
        Task IndexJobAsync(JobDocument document);

        /// <summary>Remove a job document from the index.</summary>
        Task DeleteJobAsync(int jobId);

        /// <summary>
        /// Full-text search across Title, Description, and Skills,
        /// with optional salary range filter.
        /// </summary>
        Task<IEnumerable<JobDocument>> SearchAsync(
            string keyword,
            double? minSalary = null,
            double? maxSalary = null,
            string? category = null,
            string? location = null,
            int? experienceRequired = null,
            int from = 0,
            int size = 20);

        /// <summary>Ensure the "jobs" index exists with correct mappings.</summary>
        Task EnsureIndexAsync();
    }
}
