using FoodConnectAPI.Data;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;

namespace FoodConnectAPI.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IPostTagRepository _postTagRepository;
        public TagService(ITagRepository tagRepository, IPostTagRepository postTagRepository) 
        {
            _tagRepository = tagRepository;
            _postTagRepository = postTagRepository;
        }
        public async Task<bool> DeleteAsync(int tagId)
        {
            var tag = await _tagRepository.GetTagByIdAsync(tagId);
            if (tag == null)
                return false;

            var hasPosts = await _postTagRepository.ExistsWithTagIdAsync(tagId);
            if (hasPosts)
                return false; // Or throw if needed

            await _tagRepository.DeleteAsync(tag.Id);
            await _tagRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByNameAsync(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return false;
            tagName = tagName.Trim().ToLowerInvariant();// Normalize tag name
            var tag = await _tagRepository.GetTagByNameAsync(tagName);
            if (tag == null)
                return false;
            var hasPosts = await _postTagRepository.ExistsWithTagIdAsync(tag.Id);
            if (hasPosts)
                return false; // Or throw if needed
            await _tagRepository.DeleteAsync(tag.Id);
            await _tagRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TagInfoDto>> GetAllTagsAsync()
        {
            //can be improved with caching or pagination for large datasets and not linking Posts to Tags but too lazy...
            var tags =  await _tagRepository.GetAllTagsAsync();
            //map to Dtos
            return tags.Select(tag => new TagInfoDto
            {
                Id = tag.Id,
                Name = tag.Name,
            }).ToList();
        }

        public async Task<Tag> GetTagByIdAsync(int tagId)
        {
            return await _tagRepository.GetTagByIdAsync(tagId);
        }

        public async Task<IEnumerable<Tag>> GetTagsByPostIdAsync(int postId)
        {
            return await _tagRepository.GetTagsByPostIdAsync(postId);
        }

        public async Task<Tag> GetTagByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Tag name cannot be null or empty.", nameof(name));
            }
            name = name.Trim().ToLowerInvariant(); // Normalize tag name
            return await _tagRepository.GetTagByNameAsync(name);
        }

        public async Task<List<Tag>> ResolveOrCreateTagsAsync(List<string> tagNames)
        {
            if (tagNames == null || tagNames.Count == 0)
                throw new ArgumentException("Tag names cannot be null or empty.", nameof(tagNames));

            // Normalize all tag names
            var normalizedTagNames = tagNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            // Query all existing tags in one call
            var existingTags = await _tagRepository.GetTagsByNamesAsync(normalizedTagNames);

            var existingNames = new HashSet<string>(existingTags.Select(t => t.Name));
            var newTags = new List<Tag>();

            foreach (var name in normalizedTagNames)
            {
                if (!existingNames.Contains(name))
                {
                    var newTag = new Tag { Name = name };
                    newTags.Add(newTag);
                }
            }

            if (newTags.Any())
            {
                await _tagRepository.CreateRangeAsync(newTags);
                await _tagRepository.SaveChangesAsync();
                existingTags.AddRange(newTags);
            }

            return existingTags;
        }
    }
}
