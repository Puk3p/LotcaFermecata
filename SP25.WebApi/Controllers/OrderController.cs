using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SP25.Business.ModelDTOs;
using SP25.Business.Services.Contracts;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace SP25.WebApi.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly INotificationService _notifications;

        public OrderController(IOrderService orderService, INotificationService notifications)
        {
            _orderService = orderService;
            _notifications = notifications;
        }

        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userId = dto.PlacedByUserId ?? "anonymous";
            try
            {
                var order = await _orderService.CreateOrderAsync(userId, dto);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("zone/{zone}")]
        public async Task<IActionResult> GetOrdersForZone(string zone)
        {
            try
            {
                var orders = await _orderService.GetOrdersForZoneAsync(zone);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("update-status")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusDto dto)
        {
            if (dto == null || dto.OrderId == Guid.Empty || string.IsNullOrWhiteSpace(dto.NewStatus))
                return BadRequest(new { success = false, message = "OrderId și NewStatus sunt necesare." });

            var success = await _orderService.UpdateStatusAsync(dto);
            if (!success)
                return BadRequest(new { success = false, message = "Actualizarea statusului a eșuat." });

            // (opțional) dacă vrei, poți verifica aici că userul are rol BUCĂTĂRIE din JWT/claims
            var isDone = string.Equals(dto.NewStatus, "Done", StringComparison.OrdinalIgnoreCase);
            if (isDone)
            {
                _ = _notifications
                    .NotifyOrderStatusChangedAsync(dto.OrderId.ToString(), dto.NewStatus, "BAR")
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            // TODO: log t.Exception
                        }
                    });
            }

            return Ok(new { success = true });
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                    return NotFound("Order not found");

                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] UpdateOrderDto dto)
        {
            var updated = await _orderService.UpdateOrderAsync(id, dto);
            if (!updated)
                return NotFound("Order not found");

            return Ok(new { success = true });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrders()
        {
            var orders = await _orderService.GetActiveOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedOrders()
        {
            var orders = await _orderService.GetCompletedOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("cancelled")]
        public async Task<IActionResult> GetCancelledOrders()
        {
            var orders = await _orderService.GetCancelledOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("archived")]
        public async Task<IActionResult> GetArchivedGrouped()
        {
            var grouped = await _orderService.GetArchivedGroupedAsync();
            return Ok(grouped);
        }
    }
}