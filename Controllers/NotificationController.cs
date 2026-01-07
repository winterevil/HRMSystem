using System.Security.Claims;
using HRMSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var employeeId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"
            );

            return Ok(await _service.GetMyNotificationsAsync(employeeId));
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var employeeId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"
            );

            await _service.MarkAsReadAsync(id, employeeId);
            return Ok();
        }
    }
}
