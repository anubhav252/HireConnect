using HireConnect.Interview.Entities;
using HireConnect.Interview.Services;
using Microsoft.AspNetCore.Mvc;

namespace HireConnect.Interview.Controllers
{
    [ApiController]
    [Route("api/interviews")]
    public class InterviewController : ControllerBase
    {
        private readonly IInterviewService _service;

        public InterviewController(IInterviewService service) { _service = service; }

        [HttpPost]
        public async Task<IActionResult> Schedule([FromBody] JobInterview interview) =>
            Ok(await _service.ScheduleInterviewAsync(interview));

        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            await _service.ConfirmInterviewAsync(id);
            return NoContent();
        }

        [HttpPut("{id}/reschedule")]
        public async Task<IActionResult> Reschedule(int id, [FromQuery] DateTime newTime) =>
            Ok(await _service.RescheduleInterviewAsync(id, newTime));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            await _service.CancelInterviewAsync(id);
            return NoContent();
        }

        [HttpGet("application/{applicationId}")]
        public async Task<IActionResult> GetByApplication(int applicationId) =>
            Ok(await _service.GetByApplicationIdAsync(applicationId));
    }
}
