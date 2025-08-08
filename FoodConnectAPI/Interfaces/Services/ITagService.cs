using FoodConnectAPI.Entities;

namespace FoodConnectAPI.Interfaces.Services
{
    public interface ITagService
    {
        //Todo: create methods 
        Task<List<Tag>> ResolveOrCreateTagsAsync(List<string> tagNames);
        //Delete a tag if there are no posts associated with it
        Task<bool> DeleteAsync(int tagId);
        Task<bool> DeleteByNameAsync(string tagName);
        Task<Tag> GetTagByIdAsync(int tagId);
        Task<Tag> GetTagByNameAsync(string name);
        Task<IEnumerable<Tag>> GetAllTagsAsync();


    }
}
