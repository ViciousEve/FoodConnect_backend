using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;

namespace FoodConnectAPI.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;

        public CommentService(ICommentRepository commentRepository, IPostRepository postRepository, IUserRepository userRepository)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public async Task<Comment> GetCommentByIdAsync(int commentId)
        {
            return await _commentRepository.GetCommentByIdAsync(commentId);
        }

        public async Task<IEnumerable<CommentInfoDto>> GetAllCommentsAsync()
        {
            var comments = await _commentRepository.GetAllCommentsAsync();

            if (comments == null || !comments.Any())
                return new List<CommentInfoDto>();
            // Map List<Comment> to List<CommentInfoDto>
            return comments.Select(comment => new CommentInfoDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                //commenter data
                UserId = comment.UserId,
                UserName = comment.User.UserName,
                ProfilePictureUrl = comment.User?.ProfilePictureUrl,
            });
        }

        public async Task<IEnumerable<CommentInfoDto>> GetCommentsByPostIdAsync(int postId)
        {
            var comments = await _commentRepository.GetCommentsByPostIdAsync(postId);
            if (comments == null || !comments.Any())
                return new List<CommentInfoDto>();
            // Map List<Comment> to List<CommentInfoDto>
            return comments.Select(comment => new CommentInfoDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                //commenter data
                UserId = comment.UserId,
                UserName = comment.User.UserName,
                ProfilePictureUrl = comment.User?.ProfilePictureUrl,
            });
        }

        public async Task<IEnumerable<CommentInfoDto>> GetCommentsByUserIdAsync(int userId)
        {
            var comments = await _commentRepository.GetCommentsByUserIdAsync(userId);
            if (comments == null || !comments.Any())
                return new List<CommentInfoDto>();
            // Map List<Comment> to List<CommentInfoDto>
            return comments.Select(comment => new CommentInfoDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                //commenter data
                UserId = comment.UserId,
                UserName = comment.User.UserName,
                ProfilePictureUrl = comment.User?.ProfilePictureUrl,
            });
        }

        public async Task<Comment> UpdateCommentAsync(int Id, CommentUpdateDto comment)
        {
            var existingComment = await _commentRepository.GetCommentByIdAsync(Id);
            if (existingComment == null)
                throw new ArgumentException("Comment not found", nameof(Id));
            // Update the existing comment with new values
            existingComment.Content = comment.Content;
            var updated = await _commentRepository.UpdateCommentAsync(existingComment);
            await _commentRepository.SaveChangesAsync();
            return updated;
        }

        public async Task<int> DeleteCommentAsync(int commentId)
        {
            var deleted = await _commentRepository.DeleteCommentAsync(commentId);
            await _commentRepository.SaveChangesAsync();
            return deleted;
        }

        public async Task CreateCommentAsync(int postId, int userId, CommentAddDto comment)
        {
            if(comment == null)
            {
                throw new ArgumentNullException(nameof(comment), "Comment cannot be null");
            }
            //Validate UserId
            if (userId <= 0 )
            {
                throw new ArgumentException("Invalid UserId", nameof(userId));
            }
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found", nameof(userId));
            }

            // Validate PostId
            if (postId <= 0)
            {
                throw new ArgumentException("Invalid PostId", nameof(postId));
            }
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new ArgumentException("Post not found", nameof(postId));
            }

            //Validate Content
            if (string.IsNullOrWhiteSpace(comment.Content))
            {
                throw new ArgumentException("Content cannot be empty", nameof(comment.Content));
            }

            // Create a new comment entity
            var newComment = new Comment
            {
                Content = comment.Content,
                UserId = userId,
                PostId = postId,
                CreatedAt = DateTime.UtcNow // Set the creation time to now
            };
            await _commentRepository.CreateCommentAsync(newComment);
            await _commentRepository.SaveChangesAsync();
        }
    }
}
