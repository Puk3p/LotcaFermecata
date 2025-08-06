using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SP25.Business.ModelDTOs;
using SP25.Business.Services.Contracts;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace SP25.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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


        [HttpGet("{zone}")]
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

        [HttpPatch("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] string newStatus)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
            if (!success)
                return NotFound("Order or status not found");

            return Ok("Status updated!");
        }
    }
}
