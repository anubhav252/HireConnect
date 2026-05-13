namespace HireConnect.Job.Repositories
{
    /// <summary>
    /// Repository interface for Job entity data access operations.
    /// Implementations use EF Core LINQ against JobDbContext.
    /// </summary>
    public interface IJobRepository
    {
        // ── Basic CRUD ──────────────────────────────────────────────────────────

        Task<Entities.Job?> GetByIdAsync(int id);
        Task<IEnumerable<Entities.Job>> GetAllAsync();
        Task<Entities.Job> AddAsync(Entities.Job job);
        Task<Entities.Job> UpdateAsync(Entities.Job job);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // ── Filter finders ──────────────────────────────────────────────────────

        /// <summary>Case-insensitive partial match on Title.</summary>
        Task<IEnumerable<Entities.Job>> FindByTitleAsync(string title);

        /// <summary>Exact (case-insensitive) match on Category.</summary>
        Task<IEnumerable<Entities.Job>> FindByCategoryAsync(string category);

        /// <summary>Case-insensitive partial match on Location.</summary>
        Task<IEnumerable<Entities.Job>> FindByLocationAsync(string location);

        /// <summary>All jobs posted by a specific recruiter.</summary>
        Task<IEnumerable<Entities.Job>> FindByPostedByAsync(int recruiterId);

        /// <summary>All jobs with the given Status (Active / Paused / Closed).</summary>
        Task<IEnumerable<Entities.Job>> FindByStatusAsync(string status);

        /// <summary>Jobs within a salary band — used for DB-side pre-filter.</summary>
        Task<IEnumerable<Entities.Job>> FindBySalaryRangeAsync(double minSalary, double maxSalary);

        /// <summary>Jobs whose Title contains the keyword (for DB fallback search).</summary>
        Task<IEnumerable<Entities.Job>> FindByTitleContainingAsync(string keyword);
    }
}
