using HireConnect.Shared.Events;
using HireConnect.Analytics.Data;
using HireConnect.Analytics.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace HireConnect.Analytics.Consumers
{
    public class AnalyticsEventConsumer : 
        IConsumer<JobPostedEvent>,
        IConsumer<ApplicationSubmittedEvent>,
        IConsumer<ApplicationStatusChangedEvent>
    {
        private readonly AnalyticsDbContext _dbContext;
        private readonly ILogger<AnalyticsEventConsumer> _logger;

        public AnalyticsEventConsumer(AnalyticsDbContext dbContext, ILogger<AnalyticsEventConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<JobPostedEvent> context)
        {
            await UpdatePlatformMetric("TotalJobs", 1);
            await UpdateRecruiterMetric(context.Message.RecruiterId, "TotalJobs", 1);
            _logger.LogInformation("Analytics: Incremented TotalJobs");
        }

        public async Task Consume(ConsumeContext<ApplicationSubmittedEvent> context)
        {
            await UpdatePlatformMetric("TotalApplications", 1);
            await UpdateRecruiterMetric(context.Message.RecruiterId, "TotalApplications", 1);
            _logger.LogInformation("Analytics: Incremented TotalApplications");
        }

        public async Task Consume(ConsumeContext<ApplicationStatusChangedEvent> context)
        {
            var msg = context.Message;
            
            if (msg.NewStatus == "Shortlisted" && msg.OldStatus == "Applied")
            {
                await UpdatePlatformMetric("ShortlistedCount", 1);
                await UpdateRecruiterMetric(msg.RecruiterId, "ShortlistedCount", 1);
            }
            else if (msg.NewStatus == "Rejected")
            {
                await UpdatePlatformMetric("RejectedCount", 1);
                await UpdateRecruiterMetric(msg.RecruiterId, "RejectedCount", 1);

                // If moving from a shortlisted state to rejected, decrement shortlisted count
                if (msg.OldStatus == "Shortlisted" || msg.OldStatus == "Interviewing" || msg.OldStatus == "Scheduled" || msg.OldStatus == "Confirmed")
                {
                    await UpdatePlatformMetric("ShortlistedCount", -1);
                    await UpdateRecruiterMetric(msg.RecruiterId, "ShortlistedCount", -1);
                }
            }
            else if (msg.NewStatus == "Offered")
            {
                await UpdatePlatformMetric("OfferedCount", 1);
                await UpdateRecruiterMetric(msg.RecruiterId, "OfferedCount", 1);
            }
        }

        private async Task UpdatePlatformMetric(string key, int increment)
        {
            var metric = await _dbContext.PlatformMetrics.FirstOrDefaultAsync(m => m.MetricKey == key);
            if (metric == null)
            {
                metric = new PlatformMetric { MetricKey = key, Value = increment, LastUpdated = DateTime.UtcNow };
                _dbContext.PlatformMetrics.Add(metric);
            }
            else
            {
                metric.Value += increment;
                metric.LastUpdated = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();
        }

        private async Task UpdateRecruiterMetric(int recruiterId, string name, int increment)
        {
            var metric = await _dbContext.RecruiterMetrics.FirstOrDefaultAsync(m => m.RecruiterId == recruiterId && m.MetricName == name);
            if (metric == null)
            {
                metric = new RecruiterMetric { RecruiterId = recruiterId, MetricName = name, Value = increment, LastUpdated = DateTime.UtcNow };
                _dbContext.RecruiterMetrics.Add(metric);
            }
            else
            {
                metric.Value += increment;
                metric.LastUpdated = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}
