using HireConnect.Analytics.DTOs;

namespace HireConnect.Analytics.Services
{
    public interface IAnalyticsService
    {
        Task<int> GetJobViewCountAsync(int jobId);
        Task<int> GetAppCountByJobAsync(int jobId);
        Task<double> GetViewToApplyRatioAsync(int jobId);
        Task<double> GetTimeToHireAsync(int jobId);
        Task<AnalyticsSummary> GetPipelineStatsAsync(int recruiterId);
        Task<AnalyticsSummary> GetPlatformStatsAsync();
    }
}