using FoodConnectAPI.Models;

namespace FoodConnectAPI.Interfaces.Services
{
    public interface IFollowService
    {
        Task FollowUserAsync(int followerId, int followedId);
        Task UnfollowUserAsync(int followerId, int followedId);
        Task<bool> IsFollowingAsync(int followerId, int followedId);
        Task<IEnumerable<FollowUserDto>> GetFollowersAsync(int userId);
        Task<IEnumerable<FollowUserDto>> GetFollowingAsync(int userId);
        Task<int> GetFollowerCountAsync(int userId);
        Task<int> GetFollowingCountAsync(int userId);
    }
}
