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
                .Include(t => t.PostTags)
                .Where(t => t.PostTags.Any(pt => pt.PostId == postId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> SearchTagsByNameAsync(string searchTerm)
        {
            return await _context.Tags
                .Where(t => t.Name.ToLower().Contains(searchTerm.ToLower()))
                .ToListAsync();
        }

        public async Task CreateTagAsync(Tag tag)
        {
            await _context.Tags.AddAsync(tag);
        }

        public async Task<Tag> UpdateTagAsync(Tag tag)
        {
            _context.Tags.Update(tag);
            return tag;
        }

        public async Task<bool> DeleteTagAsync(int tagId)
        {
            var tag = await _context.Tags.FindAsync(tagId);
            if (tag == null) return false;

            _context.Tags.Remove(tag);
            return true;
        }

        public async Task<bool> TagExistsAsync(string name)
        {
            return await _context.Tags.AnyAsync(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
} 