using FoodConnectAPI.Entities;
using FoodConnectAPI.Models;

namespace FoodConnectAPI.Interfaces.Services
{
    public interface IPostService
    {
        Task<PostInfoDto> GetPostByIdAsync(int postId, int? currentUserId = null);
        Task<IEnumerable<PostInfoDto>> GetAllPostsAsync(int? currentUserId = null);
        Task<IEnumerable<PostInfoDto>> GetPostsByUserIdAsync(int userId, int? currentUserId = null);
        Task<Post> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(int postId);
        Task CreatePostAsync(PostFormDto postFormDto);
    }
}
