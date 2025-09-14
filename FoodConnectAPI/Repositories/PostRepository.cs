using FoodConnectAPI.Data;
using FoodConnectAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodConnectAPI.Interfaces.Repositories;

namespace FoodConnectAPI.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Post> GetPostByIdAsync(int postId)
        {
            return await _context.Posts
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.PostLikes)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _context.Posts
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.PostLikes)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId)
        {
            return await _context.Posts
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.PostLikes)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        

        /// <summary>
        /// Deletes a post from the database. Not persisted, use SaveChangesAsync to persist changes.
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task<bool> DeletePostAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return false;
            }
            _context.Posts.Remove(post);
            return true;
        }

        /// <summary>
        /// Creates a new post in the database. Not persisted, use SaveChangesAsync to persist changes.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task CreatePostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Post> GetPostForUpdateAsync(int postId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.PostLikes)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        //Redundant method.
        //public async Task<int> CreateAndReturnIdAsync(Post post)
        //{
        //    await _context.Posts.AddAsync(post);
        //    await _context.SaveChangesAsync();
        //    return post.Id;
        //}
    }
}
