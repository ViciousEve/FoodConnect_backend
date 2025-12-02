using System.Security.Claims;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;
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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile profilePicture)
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

        [Authorize]
        [HttpPatch("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto userUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { error = "Invalid user token." });
            try
            {
                var updatedUser = await _userService.UpdateProfile(userId, userUpdateDto);
                return Ok(updatedUser);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { error = argEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }
    }
}
