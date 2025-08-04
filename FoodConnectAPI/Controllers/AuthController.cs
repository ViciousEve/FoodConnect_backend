using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;
using FoodConnectAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnectAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            if (userLoginDto == null)
            {
                return BadRequest(new { error = "Invalid login request" });
            }
            try
            {
                var userDto = await _userService.AuthenticateAsync(userLoginDto);
                if (userDto == null)
                {
                    return Unauthorized(new { error = "Invalid email or password" });
                }
                return Ok(new { message = "Login successful", user = userDto });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
        {
            
            try
            {
                await _userService.RegisterAsync(userRegisterDto);
                return Ok(new { message = "User registered successfully"});
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new {error = ex.Message});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }
    }
}
