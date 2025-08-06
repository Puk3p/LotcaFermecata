using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SP25.Business.ModelDTOs;
using SP25.Business.Services.Contracts;
using System.Security.Claims;

namespace SP25.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkZoneController : ControllerBase
    {
        private readonly IWorkZoneService _workZoneService;

        public WorkZoneController(IWorkZoneService workZoneService)
        {
            _workZoneService = workZoneService;
        }

        [Authorize]
        [HttpPost("set")]
        public async Task<IActionResult> SetZone([FromBody] SetWorkZoneDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var result = await _workZoneService.SetZoneAsync(userId, dto.Zone);
            if (!result)
                return BadRequest("Zona invalida sau user inexistent.");

            return Ok(new { message = "Zona setata cu succes!", currentZone = dto.Zone });
        }

        [Authorize]
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentZone()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var zone = await _workZoneService.GetCurrentZoneAsync(userId);
            return Ok(new { currentZone = zone?.ToString() ?? "None" });
        }
    }
}