using HireConnect.Application.Entities;
using HireConnect.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace HireConnect.Application.Controllers
{
    [ApiController]
    [Route("api/applications")]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _appService;

        public ApplicationController(IApplicationService appService)
        {
            _appService = appService;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] JobApplication application)
        {
            var result = await _appService.SubmitApplicationAsync(application);
            return Ok(result);
        }

        [HttpGet("candidate/{candidateId}")]
        public async Task<IActionResult> GetByCandidate(int candidateId)
        {
            var result = await _appService.GetByCandidateAsync(candidateId);
            return Ok(result);
        }

        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetByJob(int jobId)
        {
            var result = await _appService.GetByJobAsync(jobId);
            return Ok(result);
        }

        [HttpPut("{applicationId}/status")]
        public async Task<IActionResult> UpdateStatus(int applicationId, [FromQuery] string status)
        {
            var result = await _appService.UpdateStatusAsync(applicationId, status);
            return Ok(result);
        }

        [HttpDelete("{applicationId}/withdraw")]
        public async Task<IActionResult> Withdraw(int applicationId)
        {
            await _appService.WithdrawApplicationAsync(applicationId);
            return NoContent();
        }

        [HttpGet("{applicationId}")]
        public async Task<IActionResult> GetById(int applicationId)
        {
            var result = await _appService.GetByIdAsync(applicationId);
            return Ok(result);
        }
    }
}
