using FoodConnectAPI.Data;
using FoodConnectAPI.Interfaces;
using FoodConnectAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            return await _context.Posts.Include(p => p.User).Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _context.Posts.Include(p => p.User).Include(p => p.Comments).ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId)
        {
            return await _context.Posts.Include(p => p.User).Include(p => p.Comments)
                .Where(p => p.UserId == userId).ToListAsync();
        }

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
            postToUpdate.ImagePaths = post.ImagePaths;
            postToUpdate.Likes = post.Likes;
            postToUpdate.CreatedAt = post.CreatedAt;
            postToUpdate.UserId = post.UserId;
            return postToUpdate;
        }

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

        public async Task CreatePostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
