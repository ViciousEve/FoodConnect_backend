using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Entities;

namespace FoodConnectAPI.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;

        public LikeService(ILikeRepository likeRepository, IPostRepository postRepository, IUserRepository userRepository)
        {
            _likeRepository = likeRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public async Task<bool> LikePostAsync(int userId, int postId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (postId <= 0) throw new ArgumentException("Invalid post ID", nameof(postId));

            var alreadyLiked = await _likeRepository.UserHasLikedPostAsync(userId, postId);
            if (alreadyLiked) return false; // no-op

            var like = new Like
            {
                UserId = userId,
                PostId = postId,
                CreatedAt = DateTime.UtcNow
            };

            await _likeRepository.CreateLikeAsync(like);
            await _likeRepository.SaveChangesAsync();

            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post != null)
            {
                var postOwner = await _userRepository.GetUserByIdAsync(post.UserId);
                if (postOwner != null)
                {
                    postOwner.TotalLikesReceived++;
                    await _userRepository.UpdateUserAsync(postOwner);
                    await _userRepository.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<bool> UnlikePostAsync(int userId, int postId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (postId <= 0) throw new ArgumentException("Invalid post ID", nameof(postId));

            var deleted = await _likeRepository.DeleteLikeByUserAndPostAsync(userId, postId);
            if (!deleted) return false; // no-op
            await _likeRepository.SaveChangesAsync();

            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post != null)
            {
                var postOwner = await _userRepository.GetUserByIdAsync(post.UserId);
                if (postOwner != null && postOwner.TotalLikesReceived > 0)
                {
                    postOwner.TotalLikesReceived--;
                    await _userRepository.UpdateUserAsync(postOwner);
                    await _userRepository.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<bool> ToggleLikeAsync(int userId, int postId)
        {
            if (await _likeRepository.UserHasLikedPostAsync(userId, postId))
            {
                await UnlikePostAsync(userId, postId);
                return false; // now unliked
            }
            else
            {
                await LikePostAsync(userId, postId);
                return true; // now liked
            }
        }

        public Task<bool> UserHasLikedPostAsync(int userId, int postId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (postId <= 0) throw new ArgumentException("Invalid post ID", nameof(postId));
            return _likeRepository.UserHasLikedPostAsync(userId, postId);
        }

        public Task<int> GetLikeCountByPostIdAsync(int postId)
        {
            if (postId <= 0) throw new ArgumentException("Invalid post ID", nameof(postId));
            return _likeRepository.GetLikeCountByPostIdAsync(postId);
        }
    }
}
