using FoodConnectAPI.Entities;
using FoodConnectAPI.Models;

namespace FoodConnectAPI.Interfaces.Services
{
    public interface ICommentService
    {
        Task<Comment> GetCommentByIdAsync(int commentId);
        Task<IEnumerable<CommentInfoDto>> GetAllCommentsAsync();
        Task<IEnumerable<CommentInfoDto>> GetCommentsByPostIdAsync(int postId);
        Task<IEnumerable<CommentInfoDto>> GetCommentsByUserIdAsync(int userId);
        Task<Comment> UpdateCommentAsync(int Id, CommentUpdateDto comment);
        Task<bool> DeleteCommentAsync(int commentId);
        Task CreateCommentAsync(CommentAddDto comment);
    }
}
