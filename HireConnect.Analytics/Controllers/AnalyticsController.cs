using HireConnect.Analytics.DTOs;
using HireConnect.Analytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace HireConnect.Analytics.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _service;

        public AnalyticsController(IAnalyticsService service) { _service = service; }

        [HttpGet("recruiter/{id}")]
        public async Task<IActionResult> GetRecruiterStats(int id) => Ok(await _service.GetPipelineStatsAsync(id));


    }
}