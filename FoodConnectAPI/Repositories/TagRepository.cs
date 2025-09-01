using FoodConnectAPI.Data;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodConnectAPI.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _context;

        public TagRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Tag> GetTagByIdAsync(int tagId)
        {
            return await _context.Tags
                .Include(t => t.PostTags)
                .ThenInclude(pt => pt.Post)
                .FirstOrDefaultAsync(t => t.Id == tagId);
        }

        public async Task<Tag> GetTagByNameAsync(string name)
        {
            return await _context.Tags
                .Include(t => t.PostTags)
                .ThenInclude(pt => pt.Post)
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _context.Tags
                .Include(t => t.PostTags)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetTagsByPostIdAsync(int postId)
        {
            return await _context.Tags
                    .Where(t => t.PostTags.Any(pt => pt.PostId == postId))
                    .ToListAsync();
        }


        /// <summary>
        /// Creates a new tag in the database. Not persisted, use SaveChangesAsync to persist changes.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async Task CreateAsync(Tag tag)
        {
            await _context.Tags.AddAsync(tag);
        }

        /// <summary>
        /// Updates an existing tag in the database. Not persisted, use SaveChangesAsync to persist changes.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async Task<Tag> UpdateAsync(Tag tag)
        {
            _context.Tags.Update(tag);
            return tag;
        }

        /// <summary>
        /// Deletes a tag from the database. Not persisted, use SaveChangesAsync to persist changes.
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int tagId)
        {
            var tag = await _context.Tags.FindAsync(tagId);
            if (tag == null) return false;

            _context.Tags.Remove(tag);
            return true;
        }

        public async Task<int> DeleteAllOrphanTagsAsync()
        {
            return await _context.Tags
                .Where(t => !_context.PostTags.Any(pt => pt.TagId == t.Id))
                .ExecuteDeleteAsync();
        }

        public async Task<bool> TagExistsAsync(string name)
        {
            return await _context.Tags.AnyAsync(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a new tag and returns its ID after saving to the database. 
        /// This methods saves the changes immediately as to generate the ID.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async Task<int> CreateAndReturnIdAsync(Tag tag)
        {
            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();
            return tag.Id;
        }

        public async Task<List<Tag>> GetTagsByNamesAsync(List<string> names)
        {
            return await _context.Tags
            .Where(t => names.Contains(t.Name))
            .ToListAsync();
        }

        public async Task CreateRangeAsync(List<Tag> tags)
        {
            await _context.Tags.AddRangeAsync(tags);
        }
    }
} 