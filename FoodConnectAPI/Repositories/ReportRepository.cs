using FoodConnectAPI.Data;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodConnectAPI.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Report> GetReportByIdAsync(int reportId)
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.Post)
                .FirstOrDefaultAsync(r => r.Id == reportId);
        }

        public async Task<IEnumerable<Report>> GetReportsByPostIdAsync(int postId)
        {
            return await _context.Reports
                .Include(r => r.User)
                .Where(r => r.PostId == postId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetReportsByUserIdAsync(int userId)
        {
            return await _context.Reports
                .Include(r => r.Post)
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.Post)
                .ToListAsync();
        }

        public async Task<int> GetReportCountByPostIdAsync(int postId)
        {
            return await _context.Reports.CountAsync(r => r.PostId == postId);
        }

        public async Task CreateReportAsync(Report report)
        {
            await _context.Reports.AddAsync(report);
        }

        public async Task<bool> DeleteReportAsync(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return false;

            _context.Reports.Remove(report);
            return true;
        }

        public async Task<bool> DeleteReportsByPostIdAsync(int postId)
        {
            var reports = await _context.Reports.Where(r => r.PostId == postId).ToListAsync();
            if (!reports.Any()) return false;

            _context.Reports.RemoveRange(reports);
            return true;
        }

        public async Task<bool> UserHasReportedPostAsync(int userId, int postId)
        {
            return await _context.Reports.AnyAsync(r => r.UserId == userId && r.PostId == postId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
} 