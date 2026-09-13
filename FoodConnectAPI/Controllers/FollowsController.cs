using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodConnectAPI.Interfaces.Services;
using System.Security.Claims;

namespace FoodConnectAPI.Controllers
{
    [ApiController]
    [Route("api/users/{userId}")]
    public class FollowsController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowsController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost("follow")]
        [Authorize]
        public async Task<IActionResult> FollowUser(int userId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized();
            }

            if (currentUserId == userId)
            {
                return BadRequest(new { error = "You cannot follow yourself." });
            }

            try
            {
                await _followService.FollowUserAsync(currentUserId, userId);
                return Ok(new { message = "Successfully followed user." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        [HttpDelete("follow")]
        [Authorize]
        public async Task<IActionResult> UnfollowUser(int userId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized();
            }

            try
            {
                await _followService.UnfollowUserAsync(currentUserId, userId);
                return Ok(new { message = "Successfully unfollowed user." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        [HttpGet("followers")]
        public async Task<IActionResult> GetFollowers(int userId)
        {
            try
            {
                var followers = await _followService.GetFollowersAsync(userId);
                return Ok(followers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        [HttpGet("following")]
        public async Task<IActionResult> GetFollowing(int userId)
        {
            try
            {
                var following = await _followService.GetFollowingAsync(userId);
                return Ok(following);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }
    }
}
