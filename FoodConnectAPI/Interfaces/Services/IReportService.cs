using FoodConnectAPI.Models;

namespace FoodConnectAPI.Interfaces.Services
{
    public interface IReportService
    {
        Task<ReportInfoDto> CreateReportAsync(int userId, CreateReportDto reportDto);
        Task<IEnumerable<ReportInfoDto>> GetAllReportsAsync();
        Task<IEnumerable<ReportInfoDto>> GetReportsByPostIdAsync(int postId);
        Task<bool> DeleteReportAsync(int reportId);
        Task<bool> UserHasReportedPostAsync(int userId, int postId);
    }
}
