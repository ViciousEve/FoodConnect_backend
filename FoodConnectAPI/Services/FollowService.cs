using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;

namespace FoodConnectAPI.Services
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository _followRepository;

        public FollowService(IFollowRepository followRepository)
        {
            _followRepository = followRepository;
        }

        public async Task FollowUserAsync(int followerId, int followedId)
        {
            if (followerId == followedId)
                throw new ArgumentException("You cannot follow yourself.");

            var isFollowing = await _followRepository.UserIsFollowingAsync(followerId, followedId);
            if (isFollowing)
                return; // already following

            var follow = new Follow
            {
                FollowerId = followerId,
                FollowedId = followedId,
                FollowedAt = DateTime.UtcNow
            };

            await _followRepository.CreateFollowAsync(follow);
            await _followRepository.SaveChangesAsync();
        }

        public async Task UnfollowUserAsync(int followerId, int followedId)
        {
            var isFollowing = await _followRepository.UserIsFollowingAsync(followerId, followedId);
            if (!isFollowing)
                throw new KeyNotFoundException("Follow relationship not found.");

            await _followRepository.DeleteFollowByUsersAsync(followerId, followedId);
            await _followRepository.SaveChangesAsync();
        }

        public Task<bool> IsFollowingAsync(int followerId, int followedId)
        {
            return _followRepository.UserIsFollowingAsync(followerId, followedId);
        }

        public async Task<IEnumerable<FollowUserDto>> GetFollowersAsync(int userId)
        {
            var follows = await _followRepository.GetFollowersByUserIdAsync(userId);
            return follows.Select(f => new FollowUserDto
            {
                UserId = f.Follower.Id,
                UserName = f.Follower.UserName,
                ProfilePictureUrl = f.Follower.ProfilePictureUrl
            });
        }

        public async Task<IEnumerable<FollowUserDto>> GetFollowingAsync(int userId)
        {
            var follows = await _followRepository.GetFollowingByUserIdAsync(userId);
            return follows.Select(f => new FollowUserDto
            {
                UserId = f.Followed.Id,
                UserName = f.Followed.UserName,
                ProfilePictureUrl = f.Followed.ProfilePictureUrl
            });
        }

        public Task<int> GetFollowerCountAsync(int userId)
        {
            return _followRepository.GetFollowerCountAsync(userId);
        }

        public Task<int> GetFollowingCountAsync(int userId)
        {
            return _followRepository.GetFollowingCountAsync(userId);
        }
    }
}
