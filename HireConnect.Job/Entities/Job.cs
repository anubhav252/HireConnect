using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireConnect.Job.Entities
{
    /// <summary>
    /// Core Job entity representing a job posting in the system.
    /// Maps to the "Jobs" table in PostgreSQL via EF Core Code-First.
    /// </summary>
    [Table("Jobs")]
    public class Job
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int JobId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        /// <summary>Full-Time, Part-Time, Contract, Internship, Remote</summary>
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "SalaryMin must be non-negative.")]
        [Column(TypeName = "double precision")]
        public double SalaryMin { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "SalaryMax must be non-negative.")]
        [Column(TypeName = "double precision")]
        public double SalaryMax { get; set; }

        /// <summary>
        /// Stored as a comma-separated string in PostgreSQL; 
        /// projected to/from List&lt;string&gt; via EF Core value converter.
        /// </summary>
        public string SkillsRaw { get; set; } = string.Empty;

        [NotMapped]
        public List<string> Skills
        {
            get => string.IsNullOrWhiteSpace(SkillsRaw)
                ? new List<string>()
                : SkillsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(s => s.Trim())
                           .ToList();
            set => SkillsRaw = value != null ? string.Join(",", value) : string.Empty;
        }

        [Required]
        [Range(0, 50, ErrorMessage = "ExperienceRequired must be between 0 and 50 years.")]
        public int ExperienceRequired { get; set; }

        /// <summary>Foreign key → Recruiter's UserId from Auth-Service</summary>
        [Required]
        public int PostedBy { get; set; }

        /// <summary>Active | Paused | Closed</summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        [Required]
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(5000)]
        public string? Description { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
