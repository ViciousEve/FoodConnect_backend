using FoodConnectAPI.Entities;

namespace FoodConnectAPI.Interfaces.Repositories
{
    public interface ITagRepository
    {
        Task<Tag> GetTagByIdAsync(int tagId);
        Task<Tag> GetTagByNameAsync(string name);
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<IEnumerable<Tag>> GetTagsByPostIdAsync(int postId);
        Task<List<Tag>> GetTagsByNamesAsync(List<string> names);
        Task CreateAsync(Tag tag);
        Task<Tag> UpdateAsync(Tag tag);
        Task<bool> DeleteAsync(int tagId);
        Task<bool> TagExistsAsync(string name);
        Task SaveChangesAsync();
        Task CreateRangeAsync(List<Tag> tags);
    }
}