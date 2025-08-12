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
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.Images)
                .Include(p => p.PostLikes)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Reports)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.Images)
                .Include(p => p.PostLikes)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Reports)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.Images)
                .Include(p => p.PostLikes)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Reports)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing post in the database. Not persisted, use SaveChangesAsync to persist changes.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<Post> UpdatePostAsync(Post post)
        {
            var postToUpdate = await _context.Posts.FindAsync(post.Id);
            if (postToUpdate == null)
            {
                throw new KeyNotFoundException("Post not found");
            }
            postToUpdate.Title = post.Title;
            postToUpdate.IngredientsList = post.IngredientsList;
            postToUpdate.Description = post.Description;
            postToUpdate.Calories = post.Calories;
            postToUpdate.Likes = post.Likes; // int: number of likes, not a collection
            //postToUpdate.CreatedAt = post.CreatedAt;
            //postToUpdate.UserId = post.UserId; // ?? this should not happen 
            return postToUpdate;
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

        //Redundant method.
        //public async Task<int> CreateAndReturnIdAsync(Post post)
        //{
        //    await _context.Posts.AddAsync(post);
        //    await _context.SaveChangesAsync();
        //    return post.Id;
        //}
    }
}
