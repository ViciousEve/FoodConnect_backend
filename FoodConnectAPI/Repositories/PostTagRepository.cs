using FoodConnectAPI.Data;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodConnectAPI.Repositories
{
    public class PostTagRepository : IPostTagRepository
    {
        private readonly AppDbContext _context;

        public PostTagRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PostTag> GetPostTagAsync(int postId, int tagId)
        {
            return await _context.PostTags
                .Include(pt => pt.Post)
                .Include(pt => pt.Tag)
                .FirstOrDefaultAsync(pt => pt.PostId == postId && pt.TagId == tagId);
        }

        public async Task<IEnumerable<PostTag>> GetPostTagsByPostIdAsync(int postId)
        {
            return await _context.PostTags
                .Include(pt => pt.Tag)
                .Where(pt => pt.PostId == postId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PostTag>> GetPostTagsByTagIdAsync(int tagId)
        {
            return await _context.PostTags
                .Include(pt => pt.Post)
                .Where(pt => pt.TagId == tagId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PostTag>> GetAllPostTagsAsync()
        {
            return await _context.PostTags
                .Include(pt => pt.Post)
                .Include(pt => pt.Tag)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a PostTag without saving changes to the database. Not persisted, use SaveChangesAsync() to persist.
        /// </summary>
        /// <param name="postTag"></param>
        /// <returns></returns>
        public async Task CreatePostTagAsync(PostTag postTag)
        {
            await _context.PostTags.AddAsync(postTag);
        }

        /// <summary>
        /// Creates a PostTag and returns the IDs of the post and tag. 
        /// This method save changes to the database immediately as to generate the IDs.
        /// </summary>
        /// <param name="postTag"></param>
        /// <returns></returns>
        public async Task<(int postId, int tagId)> CreateAndReturnIdAsync(PostTag postTag)
        {
            await _context.PostTags.AddAsync(postTag);
            await _context.SaveChangesAsync();
            return (postTag.PostId, postTag.TagId);
        }

        /// <summary>
        /// Deletes a PostTag by postId and tagId. Not persisted, use SaveChangesAsync() to persist.
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        public async Task<bool> DeletePostTagAsync(int postId, int tagId)
        {
            var postTag = await _context.PostTags.FirstOrDefaultAsync(pt => pt.PostId == postId && pt.TagId == tagId);
            if (postTag == null) return false;

            _context.PostTags.Remove(postTag);
            return true;
        }

        /// <summary>
        /// Deletes all PostTags associated with a specific postId. Not persisted, use SaveChangesAsync() to persist.
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task<bool> DeletePostTagsByPostIdAsync(int postId)
        {
            var postTags = await _context.PostTags.Where(pt => pt.PostId == postId).ToListAsync();
            if (!postTags.Any()) return false;

            _context.PostTags.RemoveRange(postTags);
            return true;
        }

        public async Task<bool> PostTagExistsAsync(int postId, int tagId)
        {
            return await _context.PostTags.AnyAsync(pt => pt.PostId == postId && pt.TagId == tagId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
} 