using HireConnect.Notification.Entities;
using HireConnect.Notification.Services;
using Microsoft.AspNetCore.Mvc;

namespace HireConnect.Notification.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service) { _service = service; }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId) =>
            Ok(await _service.GetByUserAsync(userId));

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _service.MarkAsReadAsync(id);
            return NoContent();
        }

        [HttpPut("user/{userId}/read-all")]
        public async Task<IActionResult> MarkAllRead(int userId)
        {
            await _service.MarkAllReadAsync(userId);
            return NoContent();
        }


    }
}
