using FoodConnectAPI.Data;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodConnectAPI.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly AppDbContext _context;

        public LikeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Like> GetLikeByIdAsync(int likeId)
        {
            return await _context.Likes
                .Include(l => l.User)
                .Include(l => l.Post)
                .FirstOrDefaultAsync(l => l.Id == likeId);
        }

        public async Task<Like> GetLikeByUserAndPostAsync(int userId, int postId)
        {
            return await _context.Likes
                .Include(l => l.User)
                .Include(l => l.Post)
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
        }

        public async Task<IEnumerable<Like>> GetLikesByPostIdAsync(int postId)
        {
            return await _context.Likes
                .Include(l => l.User)
                .Where(l => l.PostId == postId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Like>> GetLikesByUserIdAsync(int userId)
        {
            return await _context.Likes
                .Include(l => l.Post)
                .Where(l => l.UserId == userId)
                .ToListAsync();
        }

        public async Task<int> GetLikeCountByPostIdAsync(int postId)
        {
            return await _context.Likes.CountAsync(l => l.PostId == postId);
        }

        public async Task<IEnumerable<Like>> GetAllLikesAsync()
        {
            return await _context.Likes
                .Include(l => l.User)
                .Include(l => l.Post)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new like in the database. Not persisted, use SaveChangesAsync to persist changes.
        /// </summary>
        /// <param name="like"></param>
        /// <returns></returns>
        public async Task CreateLikeAsync(Like like)
        {
            await _context.Likes.AddAsync(like);
        }

        /// <summary>
        /// Creates a new like in the database and returns the ID of the created like.
        /// This method save changes immediately as to generate the ID.
        /// </summary>
        /// <param name="like"></param>
        /// <returns></returns>
        public async Task<int> CreateAndReturnIdAsync(Like like)
        {
            await _context.Likes.AddAsync(like);
            await _context.SaveChangesAsync();
            return like.Id;
        }

        /// <summary>
        /// Deletes a like by its ID. Not persisted, use SaveChangesAsync to persist changes.
        /// </summary>
        /// <param name="likeId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteLikeAsync(int likeId)
        {
            var like = await _context.Likes.FindAsync(likeId);
            if (like == null) return false;

            _context.Likes.Remove(like);
            return true;
        }

        /// <summary>
        /// Deletes a like by user ID and post ID. Not persisted, use SaveChangesAsync to persist changes.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteLikeByUserAndPostAsync(int userId, int postId)
        {
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
            if (like == null) return false;

            _context.Likes.Remove(like);
            return true;
        }

        public async Task<bool> UserHasLikedPostAsync(int userId, int postId)
        {
            return await _context.Likes.AnyAsync(l => l.UserId == userId && l.PostId == postId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteLikeByPostIdAsync(int postId)
        {
            var likes = await _context.Likes.Where(l => l.PostId == postId).ToListAsync();
            if (!likes.Any()) return false;
            _context.Likes.RemoveRange(likes);
            return true;
        }
    }
} 