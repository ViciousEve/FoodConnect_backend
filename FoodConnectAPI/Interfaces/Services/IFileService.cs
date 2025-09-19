namespace FoodConnectAPI.Interfaces.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        void DeleteFile(string relativePath);
        bool FileExists(string relativePath);
    }
}
