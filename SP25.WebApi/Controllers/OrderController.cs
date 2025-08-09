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
            Console.WriteLine($"Received OrderId: {dto.OrderId}, NewStatus: {dto.NewStatus}");

            var success = await _orderService.UpdateOrderStatusAsync(dto.OrderId, dto.NewStatus);

            if (!success)
            {
                Console.WriteLine("FAILED to update: order not found or status invalid.");
                return NotFound("Order or status not found");
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

        [HttpGet("grouped-by-date")]
        public async Task<IActionResult> GetOrdersGroupedByDate()
        {
            try
            {
                var grouped = await _orderService.GetGroupedByDateAsync();
                return Ok(grouped);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}