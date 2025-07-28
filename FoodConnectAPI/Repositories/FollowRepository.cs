using FoodConnectAPI.Data;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodConnectAPI.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        private readonly AppDbContext _context;

        public FollowRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Follow> GetFollowByIdAsync(int followId)
        {
            return await _context.Follows
                .Include(f => f.Follower)
                .Include(f => f.Followed)
                .FirstOrDefaultAsync(f => f.Id == followId);
        }

        public async Task<Follow> GetFollowByUsersAsync(int followerId, int followedId)
        {
            return await _context.Follows
                .Include(f => f.Follower)
                .Include(f => f.Followed)
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
        }

        public async Task<IEnumerable<Follow>> GetFollowersByUserIdAsync(int userId)
        {
            return await _context.Follows
                .Include(f => f.Follower)
                .Where(f => f.FollowedId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Follow>> GetFollowingByUserIdAsync(int userId)
        {
            return await _context.Follows
                .Include(f => f.Followed)
                .Where(f => f.FollowerId == userId)
                .ToListAsync();
        }

        public async Task<int> GetFollowerCountAsync(int userId)
        {
            return await _context.Follows.CountAsync(f => f.FollowedId == userId);
        }

        public async Task<int> GetFollowingCountAsync(int userId)
        {
            return await _context.Follows.CountAsync(f => f.FollowerId == userId);
        }

        public async Task<IEnumerable<Follow>> GetAllFollowsAsync()
        {
            return await _context.Follows
                .Include(f => f.Follower)
                .Include(f => f.Followed)
                .ToListAsync();
        }

        public async Task CreateFollowAsync(Follow follow)
        {
            await _context.Follows.AddAsync(follow);
        }

        public async Task<bool> DeleteFollowAsync(int followId)
        {
            var follow = await _context.Follows.FindAsync(followId);
            if (follow == null) return false;

            _context.Follows.Remove(follow);
            return true;
        }

        public async Task<bool> DeleteFollowByUsersAsync(int followerId, int followedId)
        {
            var follow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
            if (follow == null) return false;

            _context.Follows.Remove(follow);
            return true;
        }

        public async Task<bool> UserIsFollowingAsync(int followerId, int followedId)
        {
            return await _context.Follows.AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
} 