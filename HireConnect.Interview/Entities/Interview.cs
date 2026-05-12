using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireConnect.Interview.Entities
{
    [Table("Interviews")]
    public class JobInterview
    {
        [Key]
        public int InterviewId { get; set; }
        public int ApplicationId { get; set; }
        public int? CandidateId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Mode { get; set; } = "Online";
        public string MeetLink { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public string Notes { get; set; } = string.Empty;
    }
}
