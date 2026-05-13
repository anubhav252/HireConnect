using HireConnect.Analytics.DTOs;
using HireConnect.Analytics.Data;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Analytics.Services
{
    public class AnalyticsServiceImpl : IAnalyticsService
    {
        private readonly AnalyticsDbContext _context;

        public AnalyticsServiceImpl(AnalyticsDbContext context)
        {
            _context = context;
        }

        public Task<int> GetJobViewCountAsync(int jobId) => Task.FromResult(new Random().Next(100, 1000)); // Still semi-mocked or would use Redis
        public Task<int> GetAppCountByJobAsync(int jobId) => Task.FromResult(new Random().Next(10, 100));
        public Task<double> GetViewToApplyRatioAsync(int jobId) => Task.FromResult(0.15);
        public Task<double> GetTimeToHireAsync(int jobId) => Task.FromResult(12.5);
        
        public async Task<AnalyticsSummary> GetPipelineStatsAsync(int recruiterId)
        {
            var metrics = await _context.RecruiterMetrics
                .Where(m => m.RecruiterId == recruiterId)
                .ToDictionaryAsync(m => m.MetricName, m => m.Value);
            
            int totalApps = (int)metrics.GetValueOrDefault("TotalApplications", 0);
            int shortlisted = (int)metrics.GetValueOrDefault("ShortlistedCount", 0);
            double conversionRate = totalApps > 0 ? (double)shortlisted / totalApps : 0.0;

            return new AnalyticsSummary
            {
                TotalJobs = (int)metrics.GetValueOrDefault("TotalJobs", 0),
                TotalApplications = totalApps,
                ShortlistedCount = shortlisted,
                OfferedCount = (int)metrics.GetValueOrDefault("OfferedCount", 0),
                RejectedCount = (int)metrics.GetValueOrDefault("RejectedCount", 0),
                AvgTimeToHireDays = 15.0, 
                ViewToApplyRatio = conversionRate // Using Shortlist Success Rate as the metric
            };
        }

        public async Task<AnalyticsSummary> GetPlatformStatsAsync()
        {
            var metrics = await _context.PlatformMetrics.ToDictionaryAsync(m => m.MetricKey, m => m.Value);
            
            return new AnalyticsSummary
            {
                TotalJobs = (int)metrics.GetValueOrDefault("TotalJobs", 0),
                TotalApplications = (int)metrics.GetValueOrDefault("TotalApplications", 0),
                ShortlistedCount = (int)metrics.GetValueOrDefault("ShortlistedCount", 0),
                OfferedCount = (int)metrics.GetValueOrDefault("OfferedCount", 0),
                RejectedCount = (int)metrics.GetValueOrDefault("RejectedCount", 0),
                AvgTimeToHireDays = 15.0, // Calculated or stored
                ViewToApplyRatio = 0.10
            };
        }
    }
}