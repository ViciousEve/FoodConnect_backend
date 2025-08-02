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
                return BadRequest("Invalid login request");
            }
            try
            {
                var user = await _userService.AuthenticateAsync(userLoginDto);
                if (user == null)
                {
                    return Unauthorized("Invalid email or password");
                }
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
