using FoodConnectAPI.Entities;

namespace FoodConnectAPI.Interfaces.Repositories
{
    public interface IFollowRepository
    {
        Task<Follow> GetFollowByIdAsync(int followId);
        Task<Follow> GetFollowByUsersAsync(int followerId, int followedId);
        Task<IEnumerable<Follow>> GetFollowersByUserIdAsync(int userId);
        Task<IEnumerable<Follow>> GetFollowingByUserIdAsync(int userId);
        Task<int> GetFollowerCountAsync(int userId);
        Task<int> GetFollowingCountAsync(int userId);
        Task<IEnumerable<Follow>> GetAllFollowsAsync();
        Task CreateFollowAsync(Follow follow);
        Task<bool> DeleteFollowAsync(int followId);
        Task<bool> DeleteFollowByUsersAsync(int followerId, int followedId);
        Task<bool> UserIsFollowingAsync(int followerId, int followedId);
        Task SaveChangesAsync();
    }
}