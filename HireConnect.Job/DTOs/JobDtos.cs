using System.ComponentModel.DataAnnotations;

namespace HireConnect.Job.DTOs
{
    /// <summary>
    /// Inbound DTO for creating or updating a job.
    /// The entity is never exposed directly to the API consumer.
    /// </summary>
    public class JobRequestDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required.")]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "SalaryMin must be non-negative.")]
        public double SalaryMin { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "SalaryMax must be non-negative.")]
        public double SalaryMax { get; set; }

        public List<string> Skills { get; set; } = new();

        [Range(0, 50, ErrorMessage = "ExperienceRequired must be between 0 and 50.")]
        public int ExperienceRequired { get; set; }

        /// <summary>Set by the service from the JWT claim — not trusted from the body.</summary>
        public int PostedBy { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        [MaxLength(5000)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Outbound DTO returned to API consumers.
    /// Maps from the Job entity, keeping internal fields hidden.
    /// </summary>
    public class JobResponseDto
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
        public DateTime? UpdatedAt { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>Wrapper for paginated or list API responses.</summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success") =>
            new() { Success = true, Message = message, Data = data };

        public static ApiResponse<T> Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
