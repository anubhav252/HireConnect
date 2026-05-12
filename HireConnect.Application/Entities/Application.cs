using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireConnect.Application.Entities
{
    [Table("Applications")]
    public class JobApplication
    {
        [Key]
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public string? JobTitle { get; set; }
        public int RecruiterId { get; set; }
        public int CandidateId { get; set; }
        public string? CandidateEmail { get; set; }
        public DateTime AppliedAt { get; set; }
        public string Status { get; set; } = "Applied";
        public string CoverLetter { get; set; } = string.Empty;
        public string ResumeUrl { get; set; } = string.Empty;
    }
}
