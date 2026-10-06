using FoodConnectAPI.Interfaces.Services;

namespace FoodConnectAPI.Services
{
    public class FileService : IFileService
    {
        private readonly string _rootPath;
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };
        
        public FileService(string rootPath = null)
        {
            _rootPath = rootPath ?? Directory.GetCurrentDirectory();
        }

        public void ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("File is empty or null.");

            if (file.Length > MaxFileSize)
                throw new InvalidOperationException($"File {file.FileName} exceeds the maximum size of {MaxFileSize / (1024 * 1024)} MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            {
                throw new InvalidOperationException($"File {file.FileName} has an invalid or unsupported extension.");
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                throw new InvalidOperationException($"File {file.FileName} is not a valid image.");
            }
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return;

            var fullPath = Path.Combine(_rootPath, "wwwroot", relativePath.TrimStart('/'));
            if(File.Exists(fullPath)) 
                File.Delete(fullPath);
        }

        public bool FileExists(string relativePath)
        {
            if(string.IsNullOrEmpty(relativePath))
                return false;

            var fullPath = Path.Combine(_rootPath, "wwwroot", relativePath.TrimStart('/'));
            return File.Exists(fullPath);
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_rootPath, "wwwroot", folder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Validate file size
            if (file.Length > MaxFileSize)
                throw new InvalidOperationException($"File {file.FileName} exceeds the maximum size of {MaxFileSize / (1024 * 1024)} MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqeFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, uniqeFileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/{folder}/{uniqeFileName}";
        }
    }
}
