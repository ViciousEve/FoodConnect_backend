using FoodConnectAPI.Entities;
using FoodConnectAPI.Models;

namespace FoodConnectAPI.Interfaces.Services
{
    public interface IPostService
    {
        Task<PostInfoDto> GetPostByIdAsync(int postId, int? currentUserId = null);
        Task<IEnumerable<PostInfoDto>> GetAllPostsAsync(int? currentUserId = null);
        Task<IEnumerable<PostInfoDto>> GetPostsByUserIdAsync(int userId, int? currentUserId = null);
        Task UpdatePostAsync(int postId, PostFormDto post);
        Task<bool> DeletePostAsync(int postId);
        Task CreatePostAsync(int userId, PostFormDto postFormDto);
        Task<bool> IsOwnerAsync(int userId, int postId);
    }
}
