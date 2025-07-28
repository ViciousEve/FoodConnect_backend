using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;

namespace FoodConnectAPI.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<Comment> GetCommentByIdAsync(int commentId)
        {
            return await _commentRepository.GetCommentByIdAsync(commentId);
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await _commentRepository.GetAllCommentsAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(int postId)
        {
            return await _commentRepository.GetCommentsByPostIdAsync(postId);
        }

        public async Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(int userId)
        {
            return await _commentRepository.GetCommentsByUserIdAsync(userId);
        }

        public async Task<Comment> UpdateCommentAsync(Comment comment)
        {
            var updated = await _commentRepository.UpdateCommentAsync(comment);
            await _commentRepository.SaveChangesAsync();
            return updated;
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var deleted = await _commentRepository.DeleteCommentAsync(commentId);
            await _commentRepository.SaveChangesAsync();
            return deleted;
        }

        public async Task CreateCommentAsync(Comment comment)
        {
            await _commentRepository.CreateCommentAsync(comment);
            await _commentRepository.SaveChangesAsync();
        }
    }
}
