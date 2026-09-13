using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;

namespace FoodConnectAPI.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;

        public ReportService(IReportRepository reportRepository, IPostRepository postRepository, IUserRepository userRepository)
        {
            _reportRepository = reportRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public async Task<ReportInfoDto> CreateReportAsync(int userId, CreateReportDto reportDto)
        {
            if (await _reportRepository.UserHasReportedPostAsync(userId, reportDto.PostId))
            {
                throw new InvalidOperationException("You have already reported this post.");
            }

            var post = await _postRepository.GetPostByIdAsync(reportDto.PostId);
            if (post == null)
            {
                throw new KeyNotFoundException("Post not found.");
            }

            var user = await _userRepository.GetUserByIdAsync(userId);

            var report = new Report
            {
                UserId = userId,
                PostId = reportDto.PostId,
                Reason = reportDto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            await _reportRepository.CreateReportAsync(report);
            await _reportRepository.SaveChangesAsync();

            return new ReportInfoDto
            {
                Id = report.Id,
                UserId = userId,
                UserName = user?.UserName ?? "Unknown",
                PostId = report.PostId,
                Reason = report.Reason,
                CreatedAt = report.CreatedAt
            };
        }

        public async Task<IEnumerable<ReportInfoDto>> GetAllReportsAsync()
        {
            var reports = await _reportRepository.GetAllReportsAsync();
            return reports.Select(r => new ReportInfoDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User?.UserName ?? "Unknown",
                PostId = r.PostId,
                Reason = r.Reason,
                CreatedAt = r.CreatedAt
            });
        }

        public async Task<IEnumerable<ReportInfoDto>> GetReportsByPostIdAsync(int postId)
        {
            var reports = await _reportRepository.GetReportsByPostIdAsync(postId);
            return reports.Select(r => new ReportInfoDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User?.UserName ?? "Unknown",
                PostId = r.PostId,
                Reason = r.Reason,
                CreatedAt = r.CreatedAt
            });
        }

        public async Task<bool> DeleteReportAsync(int reportId)
        {
            var result = await _reportRepository.DeleteReportAsync(reportId);
            await _reportRepository.SaveChangesAsync();
            return result > 0;
        }

        public Task<bool> UserHasReportedPostAsync(int userId, int postId)
        {
            return _reportRepository.UserHasReportedPostAsync(userId, postId);
        }
    }
}
