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

        public async Task CreateLikeAsync(Like like)
        {
            await _context.Likes.AddAsync(like);
        }

        public async Task<bool> DeleteLikeAsync(int likeId)
        {
            var like = await _context.Likes.FindAsync(likeId);
            if (like == null) return false;

            _context.Likes.Remove(like);
            return true;
        }

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
    }
} 