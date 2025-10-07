using System.Security.Claims;
using FoodConnectAPI.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnectAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpPatch("profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] IFormFile profilePicture)
        {
            if (profilePicture == null)
                return BadRequest(new { error = "Profile picture is required." });
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { error = "Invalid user token." });
            try
            {
                await _userService.UpdateProfilePicture(userId, profilePicture);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }
    }
}
