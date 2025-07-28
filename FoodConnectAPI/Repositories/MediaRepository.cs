using FoodConnectAPI.Data;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodConnectAPI.Repositories
{
    public class MediaRepository : IMediaRepository
    {
        private readonly AppDbContext _context;

        public MediaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Media> GetMediaByIdAsync(int mediaId)
        {
            return await _context.Media
                .Include(m => m.Post)
                .FirstOrDefaultAsync(m => m.Id == mediaId);
        }

        public async Task<IEnumerable<Media>> GetMediaByPostIdAsync(int postId)
        {
            return await _context.Media
                .Where(m => m.PostId == postId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Media>> GetAllMediaAsync()
        {
            return await _context.Media
                .Include(m => m.Post)
                .ToListAsync();
        }

        public async Task CreateMediaAsync(Media media)
        {
            await _context.Media.AddAsync(media);
        }

        public async Task<Media> UpdateMediaAsync(Media media)
        {
            _context.Media.Update(media);
            return media;
        }

        public async Task<bool> DeleteMediaAsync(int mediaId)
        {
            var media = await _context.Media.FindAsync(mediaId);
            if (media == null) return false;

            _context.Media.Remove(media);
            return true;
        }

        public async Task<bool> DeleteMediaByPostIdAsync(int postId)
        {
            var mediaList = await _context.Media.Where(m => m.PostId == postId).ToListAsync();
            if (!mediaList.Any()) return false;

            _context.Media.RemoveRange(mediaList);
            return true;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
} 