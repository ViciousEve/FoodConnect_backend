using FoodConnectAPI.Entities;

namespace FoodConnectAPI.Interfaces.Repositories
{
    public interface ILikeRepository
    {
        Task<Like> GetLikeByIdAsync(int likeId);
        Task<Like> GetLikeByUserAndPostAsync(int userId, int postId);
        Task<IEnumerable<Like>> GetLikesByPostIdAsync(int postId);
        Task<IEnumerable<Like>> GetLikesByUserIdAsync(int userId);
        Task<int> GetLikeCountByPostIdAsync(int postId);
        Task<IEnumerable<Like>> GetAllLikesAsync();
        Task CreateLikeAsync(Like like);
        Task<bool> DeleteLikeAsync(int likeId);
        Task<bool> DeleteLikeByUserAndPostAsync(int userId, int postId);
        Task<bool> UserHasLikedPostAsync(int userId, int postId);
        Task<bool> DeleteLikeByPostIdAsync(int postId);
        Task SaveChangesAsync();
    }
}