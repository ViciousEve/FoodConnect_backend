using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Services;
using Microsoft.AspNetCore.Http;

namespace FoodConnectAPI.Test.Services
{
    public class FileServiceTest : IDisposable
    {
        private readonly string _tempRoot;
        private FileService _fileService;

        public FileServiceTest()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "FileServiceTests", Path.GetRandomFileName());
            Directory.CreateDirectory(_tempRoot);

            _fileService = new FileService(_tempRoot);
        }

        [Fact]
        public async Task SaveFileAsync_Should_Save_File_And_Return_RelativePath()
        {
            // Arrange
            var content = "dummy text";
            var fileName = "test.txt";
            var formFile = new FormFile(
                new MemoryStream(Encoding.UTF8.GetBytes(content)),
                0,
                content.Length,
                "Data",
                fileName
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            // Act
            var relativePath = await _fileService.SaveFileAsync(formFile, "Uploads");

            // Assert
            relativePath.Should().StartWith("/Uploads/");
            var fullPath = Path.Combine(_tempRoot, "wwwroot", relativePath.TrimStart('/'));
            File.Exists(fullPath).Should().BeTrue();

            var fileText = await File.ReadAllTextAsync(fullPath);
            fileText.Should().Be(content);
        }

        [Fact]
        public void FileExists_Should_Return_True_When_File_Is_Present()
        {
            // Arrange
            var relativePath = "/Uploads/existing.txt";
            var fullPath = Path.Combine(_tempRoot, "wwwroot", "Uploads", "existing.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, "sample");

            // Act
            var exists = _fileService.FileExists(relativePath);

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public void DeleteFile_Should_Remove_File()
        {
            // Arrange
            var relativePath = "/Uploads/delete.txt";
            var fullPath = Path.Combine(_tempRoot, "wwwroot", "Uploads", "delete.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, "to delete");

            // Act
            _fileService.DeleteFile(relativePath);

            // Assert
            File.Exists(fullPath).Should().BeFalse();
        }
        // Cleanup after all tests in this class
        public void Dispose()
        {
            if (Directory.Exists(_tempRoot))
            {
                try
                {
                    Directory.Delete(_tempRoot, recursive: true);
                }
                catch
                {
                    // Swallow exceptions if files are still locked,
                    // keeps tests from failing on cleanup.
                }
            }
        }
    }
}
