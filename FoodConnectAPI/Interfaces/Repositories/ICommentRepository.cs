using FoodConnectAPI.Entities;

namespace FoodConnectAPI.Interfaces.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment> GetCommentByIdAsync(int commentId);
        Task<IEnumerable<Comment>> GetAllCommentsAsync();
        Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(int postId);
        Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(int userId);
        Task<Comment> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int commentId);
        Task<bool> DeleteCommentsByPostIdAsync(int postId);
        Task<bool> DeleteCommentsByUserIdAsync(int userId);
        Task CreateCommentAsync(Comment comment);
        Task SaveChangesAsync();
    }
}
