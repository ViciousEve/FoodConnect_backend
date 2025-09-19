using FoodConnectAPI.Entities;

namespace FoodConnectAPI.Interfaces.Repositories
{
    public interface IPostRepository
    {
        Task<Post> GetPostByIdAsync(int postId);
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId);
        Task<Post> GetPostForUpdateAsync(int postId);
        Task<bool> DeletePostAsync(int postId);
        Task CreatePostAsync(Post post);
        Task SaveChangesAsync();
    }
}
