using FoodConnectAPI.Entities;
using FoodConnectAPI.Models;

namespace FoodConnectAPI.Interfaces.Services
{
    public interface IPostService
    {
        Task<PostInfoDto> GetPostByIdAsync(int postId);
        Task<IEnumerable<PostInfoDto>> GetAllPostsAsync();
        Task<IEnumerable<PostInfoDto>> GetPostsByUserIdAsync(int userId);
        Task<Post> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(int postId);
        Task CreatePostAsync(int userId, PostAddDto post);
    }
}
