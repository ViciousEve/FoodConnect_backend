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

        //[HttpGet("comments/by-post/{postId}")]
        //public async Task<IActionResult> GetCommentsByPostId(int postId)
        //{
        //    if (postId <= 0)
        //    {
        //        return BadRequest(new { error = "Invalid post ID." });
        //    }
        //    try
        //    {
        //        var comments = await _commentService.GetCommentsByPostIdAsync(postId);
        //        return Ok(comments);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
        //    }
        //}

        //[HttpPost("comments")]
        //public async Task<IActionResult> AddComment([FromBody] CommentAddDto comment)
        //{
        //    if(!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    await _commentService.CreateCommentAsync(comment);

        //    return Ok(new { message = "Comment added successfully." });
        //}

        [HttpPatch("/{commentId}")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] CommentUpdateDto comment)
        {
            if (commentId <= 0 || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedComment = await _commentService.UpdateCommentAsync(commentId, comment);
            if (updatedComment == null)
            {
                return NotFound(new { error = "Comment not found." });
            }
            return Ok(updatedComment);
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            if (commentId <= 0)
            {
                return BadRequest(new { error = "Invalid comment ID." });
            }
            var isDeleted = await _commentService.DeleteCommentAsync(commentId);
            if (!isDeleted)
            {
                return NotFound(new { error = "Comment not found." });
            }
            return Ok(new { message = "Comment deleted successfully." });
        }
    }
}
