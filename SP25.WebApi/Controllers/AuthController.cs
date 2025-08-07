using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SP25.Business.ModelDTOs;
using SP25.Domain.Models;

namespace SP25.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = $"{model.Username}@lotcafermecata.ro"
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                    return Unauthorized("Invalid credentials");

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded)
                    return Unauthorized("Invalid credentials");

                var token = _jwtTokenService.GenerateToken(user.Id, user.UserName);

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal error", ex.Message, ex.StackTrace });
            }
        }

    }
}