using FoodConnectAPI.Models;

namespace FoodConnectAPI.Interfaces
{
    public interface IPostRepository
    {
        Task<Post> GetPostByIdAsync(int postId);
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId);
        Task<Post> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(int postId);
        Task CreatePostAsync(Post post);
        Task SaveChangesAsync();
    }
}
