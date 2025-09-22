using FoodConnectAPI.Entities;

namespace FoodConnectAPI.Interfaces.Repositories
{
    public interface IReportRepository
    {
        Task<Report> GetReportByIdAsync(int reportId);
        Task<IEnumerable<Report>> GetReportsByPostIdAsync(int postId);
        Task<IEnumerable<Report>> GetReportsByUserIdAsync(int userId);
        Task<IEnumerable<Report>> GetAllReportsAsync();
        Task<int> GetReportCountByPostIdAsync(int postId);
        Task CreateReportAsync(Report report);
        Task<int> DeleteReportAsync(int reportId);
        Task<int> DeleteReportsByPostIdAsync(int postId);
        Task<bool> UserHasReportedPostAsync(int userId, int postId);
        Task SaveChangesAsync();
    }
}