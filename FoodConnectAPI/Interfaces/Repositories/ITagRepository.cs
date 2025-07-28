using FoodConnectAPI.Entities;

namespace FoodConnectAPI.Interfaces.Repositories
{
    public interface ITagRepository
    {
        Task<Tag> GetTagByIdAsync(int tagId);
        Task<Tag> GetTagByNameAsync(string name);
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<IEnumerable<Tag>> GetTagsByPostIdAsync(int postId);
        Task<IEnumerable<Tag>> SearchTagsByNameAsync(string searchTerm);
        Task CreateTagAsync(Tag tag);
        Task<Tag> UpdateTagAsync(Tag tag);
        Task<bool> DeleteTagAsync(int tagId);
        Task<bool> TagExistsAsync(string name);
        Task SaveChangesAsync();
    }
}