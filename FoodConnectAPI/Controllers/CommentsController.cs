using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnectAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [Authorize]
        [HttpPatch("{commentId}")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] CommentUpdateDto comment)
        {
            if (commentId <= 0 || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { error = "Invalid user token." });

            var existingComment = await _commentService.GetCommentByIdAsync(commentId);
            if (existingComment == null)
            {
                return NotFound(new { error = "Comment not found." });
            }

            if (existingComment.UserId != userId)
            {
                return StatusCode(403, new { error = "You are not the owner of this comment." });
            }

            var updatedComment = await _commentService.UpdateCommentAsync(commentId, comment);
            return Ok(updatedComment);
        }

        [Authorize]
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            if (commentId <= 0)
            {
                return BadRequest(new { error = "Invalid comment ID." });
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { error = "Invalid user token." });

            var existingComment = await _commentService.GetCommentByIdAsync(commentId);
            if (existingComment == null)
            {
                return NotFound(new { error = "Comment not found." });
            }

            if (existingComment.UserId != userId)
            {
                return StatusCode(403, new { error = "You are not the owner of this comment." });
            }

            var isDeleted = await _commentService.DeleteCommentAsync(commentId);
            return Ok(new { message = "Comment deleted successfully." });
        }
    }
}
