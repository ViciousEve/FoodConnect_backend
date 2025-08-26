using FoodConnectAPI.Entities;
using FoodConnectAPI.Models;

namespace FoodConnectAPI.Interfaces.Services
{
    public interface ITagService
    {
        Task<List<Tag>> ResolveOrCreateTagsAsync(List<string> tagNames);
        //Delete a tag if there are no posts associated with it
        Task<bool> DeleteAsync(int tagId);
        Task<bool> DeleteByNameAsync(string tagName);
        Task<Tag> GetTagByIdAsync(int tagId);
        Task<IEnumerable<Tag>> GetTagsByPostIdAsync(int postId);
        Task<Tag> GetTagByNameAsync(string name);
        Task<IEnumerable<TagInfoDto>> GetAllTagsAsync();


    }
}
