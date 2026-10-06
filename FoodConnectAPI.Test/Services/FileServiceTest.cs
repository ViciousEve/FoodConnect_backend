using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Services;
using Microsoft.AspNetCore.Http;
using Moq;

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

        [Fact]
        public void ValidateImageFile_WithValidImage_ShouldNotThrowException()
        {
            // Arrange
            var content = new byte[] { 0xFF, 0xD8, 0xFF };
            var formFile = new FormFile(
                new MemoryStream(content),
                0,
                content.Length,
                "file",
                "photo.jpg"
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            Action act = () => _fileService.ValidateImageFile(formFile);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateImageFile_WithNullFile_ShouldThrowInvalidOperationException()
        {
            // Act
            Action act = () => _fileService.ValidateImageFile(null!);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("File is empty or null.");
        }

        [Fact]
        public void ValidateImageFile_WithEmptyFile_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var formFile = new FormFile(
                new MemoryStream(Array.Empty<byte>()),
                0,
                0,
                "file",
                "photo.png"
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };

            // Act
            Action act = () => _fileService.ValidateImageFile(formFile);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("File is empty or null.");
        }

        [Fact]
        public void ValidateImageFile_WhenFileExceedsMaxSize_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(11 * 1024 * 1024); // 11 MB > 10 MB
            mockFile.Setup(f => f.FileName).Returns("large.jpg");

            // Act
            Action act = () => _fileService.ValidateImageFile(mockFile.Object);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("File large.jpg exceeds the maximum size of 10 MB.");
        }

        [Fact]
        public void ValidateImageFile_WithInvalidFileExtension_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var content = new byte[] { 0x20, 0x20, 0x20 };
            var formFile = new FormFile(
                new MemoryStream(content),
                0,
                content.Length,
                "file",
                "profile.txt"
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            // Act
            Action act = () => _fileService.ValidateImageFile(formFile);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("File profile.txt has an invalid or unsupported extension.");
        }

        [Fact]
        public void ValidateImageFile_WithInvalidMimeType_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var content = new byte[] { 0x20, 0x20, 0x20 };
            var formFile = new FormFile(
                new MemoryStream(content),
                0,
                content.Length,
                "file",
                "profile.jpg"
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            // Act
            Action act = () => _fileService.ValidateImageFile(formFile);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("File profile.jpg is not a valid image.");
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
