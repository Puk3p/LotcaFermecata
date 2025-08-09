using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SP25.Business.ModelDTOs;
using SP25.Business.Services.Contracts;

namespace SP25.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notifications;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(INotificationService notifications, ILogger<NotificationsController> logger)
        {
            _notifications = notifications;
            _logger = logger;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] PushSubscribeDto dto)
        {
            _logger.LogInformation("📩 SUBSCRIBE request received");

            if (dto == null || dto.Subscription == null)
            {
                _logger.LogWarning("⚠ Invalid payload received at /subscribe");
                return BadRequest("Invalid payload.");
            }

            _logger.LogInformation("UserId={UserId}, Role={Role}, Endpoint={Endpoint}",
                dto.UserId, dto.Role, dto.Subscription.Endpoint);

            await _notifications.SubscribeAsync(dto);

            _logger.LogInformation("✅ Subscription saved successfully");
            return Ok(new { success = true });
        }

        [HttpPost("order-status-changed")]
        public async Task<IActionResult> OrderStatusChanged([FromBody] OrderStatusChangedDto dto)
        {
            _logger.LogInformation("📩 ORDER STATUS CHANGE request received");

            if (string.IsNullOrWhiteSpace(dto?.OrderId) || string.IsNullOrWhiteSpace(dto?.NewStatus))
            {
                _logger.LogWarning("⚠ Missing OrderId or NewStatus in request");
                return BadRequest("OrderId and NewStatus are required.");
            }

            _logger.LogInformation("🔄 Notifying subscribers for Order={OrderId}, NewStatus={NewStatus}",
                dto.OrderId, dto.NewStatus);

            await _notifications.NotifyOrderStatusChangedAsync(dto.OrderId, dto.NewStatus);

            _logger.LogInformation("✅ Notification send attempt finished");
            return Ok(new { success = true });
        }

        [HttpPost("test")]
        public async Task<IActionResult> Test([FromBody] string? role = "BAR")
        {
            _logger.LogInformation("🧪 TEST push initiated for Role={Role}", role);

            await _notifications.NotifyOrderStatusChangedAsync("TEST-ORDER", "Completed");

            _logger.LogInformation("✅ Test push sent (if subscriptions exist)");
            return Ok("Trimis (dacă există subscriptions).");
        }
    }
}
