using System.ComponentModel.DataAnnotations;

namespace HireConnect.Analytics.Entities
{
    public class PlatformMetric
    {
        [Key]
        public string MetricKey { get; set; } = string.Empty; // e.g., "TotalJobs", "TotalApplications"
        public long Value { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class RecruiterMetric
    {
        [Key]
        public int Id { get; set; }
        public int RecruiterId { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public long Value { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
