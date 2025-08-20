using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnectAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        public PostsController(IPostService postService, ICommentService commentService)
        {
            _postService = postService;
            _commentService = commentService;
        }

        // GET /api/posts
        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            try
            {
                var posts = await _postService.GetAllPostsAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        // POST /api/posts
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] PostFormDto postFormDto)
        {
            if (string.IsNullOrWhiteSpace(postFormDto.Title) || string.IsNullOrWhiteSpace(postFormDto.IngredientsList) || string.IsNullOrWhiteSpace(postFormDto.Description))
            {
                return BadRequest(new { error = "Title, ingredients, and description are required." });
            }
            try
            {
               
                await _postService.CreatePostAsync(postFormDto);
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
            {
                return BadRequest(new { error = "Invalid post ID." });
            }
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
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> AddComment(int postId, [FromBody] CommentAddDto comment)
        {
            if (postId <= 0 || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                comment.PostId = postId; // Ensure the comment is associated with the correct post
                await _commentService.CreateCommentAsync(comment);
                return Ok(new { message = "Comment added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }
    }
}
