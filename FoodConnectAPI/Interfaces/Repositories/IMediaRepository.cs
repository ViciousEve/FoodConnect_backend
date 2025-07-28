using FoodConnectAPI.Entities;

namespace FoodConnectAPI.Interfaces.Repositories
{
    public interface IMediaRepository
    {
        Task<Media> GetMediaByIdAsync(int mediaId);
        Task<IEnumerable<Media>> GetMediaByPostIdAsync(int postId);
        Task<IEnumerable<Media>> GetAllMediaAsync();
        Task CreateMediaAsync(Media media);
        Task<Media> UpdateMediaAsync(Media media);
        Task<bool> DeleteMediaAsync(int mediaId);
        Task<bool> DeleteMediaByPostIdAsync(int postId);
        Task SaveChangesAsync();
    }
}