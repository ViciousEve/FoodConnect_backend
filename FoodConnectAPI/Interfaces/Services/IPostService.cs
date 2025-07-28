namespace FoodConnectAPI.Interfaces.Services
{
    public interface IPostService
    {
        Task<Post> GetPostByIdAsync(int postId);
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId);
        Task<Post> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(int postId);
        Task CreatePostAsync(Post post);
    }
}
