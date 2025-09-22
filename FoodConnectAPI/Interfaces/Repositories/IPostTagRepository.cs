using FoodConnectAPI.Entities;

namespace FoodConnectAPI.Interfaces.Repositories
{
    public interface IPostTagRepository
    {
        Task<PostTag> GetPostTagAsync(int postId, int tagId);
        Task<IEnumerable<PostTag>> GetPostTagsByPostIdAsync(int postId);
        Task<IEnumerable<PostTag>> GetPostTagsByTagIdAsync(int tagId);
        Task<IEnumerable<PostTag>> GetAllPostTagsAsync();
        Task CreatePostTagAsync(PostTag postTag);
        Task<int> DeletePostTagAsync(int postId, int tagId);
        Task<int> DeleteAllByPostIdAsync(int postId);
        Task<bool> PostTagExistsAsync(int postId, int tagId);
        Task<bool> ExistsWithTagIdAsync(int tagId);
        Task SaveChangesAsync();
    }
}