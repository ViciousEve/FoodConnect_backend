namespace FoodConnectAPI.Interfaces.Services
{
    public interface ILikeService
    {
        Task<bool> LikePostAsync(int userId, int postId);
        Task<bool> UnlikePostAsync(int userId, int postId);

        /// <summary>
        /// Toggles like status for a post by a user. Returns true if now liked, false if unliked.
        /// </summary>
        Task<bool> ToggleLikeAsync(int userId, int postId);
        Task<bool> UserHasLikedPostAsync(int userId, int postId);
        Task<int> GetLikeCountByPostIdAsync(int postId);
        
        // Optional helpers
        // Task<IEnumerable<Like>> GetLikesByPostIdAsync(int postId);
        // Task<IEnumerable<Like>> GetLikesByUserIdAsync(int userId);
    }
}
