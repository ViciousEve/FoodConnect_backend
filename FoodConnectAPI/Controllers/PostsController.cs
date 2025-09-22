using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodConnectAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly ILikeService _likeService;
        public PostsController(IPostService postService, ICommentService commentService, ILikeService likeService)
        {
            _postService = postService;
            _commentService = commentService;
            _likeService = likeService;
        }

        // GET /api/posts
        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            try
            {
                int? currentUserId = null;

                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
                    {
                        currentUserId = userId;
                    }
                }

                var posts = await _postService.GetAllPostsAsync(currentUserId);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        // POST /api/posts
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] PostFormDto postFormDto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { error = "Invalid user token." });


            try
            {

                await _postService.CreatePostAsync(userId, postFormDto);
                return Ok(new { message = "Post created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        // GET /api/posts/{postId}/comments
        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetCommentsByPostId(int postId)
        {
            if (postId <= 0)            
                return BadRequest(new { error = "Invalid post ID." });
            try
            {
                var comments = await _commentService.GetCommentsByPostIdAsync(postId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        //POST /api/posts/{postId}/comments
        [Authorize]
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> AddComment(int postId, [FromBody] CommentAddDto comment)
        {
            if (postId <= 0 || !ModelState.IsValid)
                return BadRequest(ModelState);
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { error = "Invalid user token." });

            try
            {
                await _commentService.CreateCommentAsync(postId, userId, comment);
                return Ok(new { message = "Comment added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        // POST /api/posts/{postId}/likes
        [Authorize]
        [HttpPost("{postId}/likes")]
        public async Task<IActionResult> ToggleLikePost(int postId)
        {
            if (postId <= 0)
                return BadRequest(new { error = "Invalid post ID." });
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { error = "Invalid user token." });

            try
            {
                bool isLiked = await _likeService.ToggleLikeAsync(userId, postId);
                
                return Ok(isLiked);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        // PUT /api/posts/{postId}
        [Authorize]
        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, [FromForm] PostFormDto postFormDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { error = "Invalid user token." });

            if (postId <= 0) 
                return BadRequest(new { error = "Invalid postId" });

            if (!await _postService.IsOwnerAsync(userId, postId))
                return StatusCode(403, new { error = "You are not the owner of this post." });

            try
            {
                await _postService.UpdatePostAsync(postId, postFormDto);
                return Ok(new { message = "Post updated successfully"});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        // DELETE /api/posts/{postId}
        [Authorize]
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { error = "Invalid user token." });

            if (postId <= 0)
                return BadRequest(new { error = "Invalid postId" });

            if (!await _postService.IsOwnerAsync(userId, postId))
                return StatusCode(403, new { error = "You are not the owner of this post." });

            try
            {
                await _postService.DeletePostAsync(postId);
                return Ok(new { message = "Post deleted successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + e.Message });
            }

        }
    }
}
