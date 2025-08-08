using FoodConnectAPI.Services;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Data;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using FoodConnectAPI.Test.Factories;

namespace FoodConnectAPI.Test.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPostRepository> _mockPostRepository;
        private readonly Mock<ICommentRepository> _mockCommentRepository;
        private readonly Mock<AppDbContext> _mockDbContext;
        private readonly Mock<IConfiguration> _mockConfiguration;
        //private readonly Mock<IDbContextTransaction> _mockTransaction;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPostRepository = new Mock<IPostRepository>();
            _mockCommentRepository = new Mock<ICommentRepository>();

            _mockConfiguration = MockConfigurationFactory.CreateJwtMock();

            // Create a real in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var realDbContext = new AppDbContext(options);
            _mockDbContext = new Mock<AppDbContext>(options);

            // Setup the mock to use the real context's database
            _mockDbContext.Setup(x => x.Database).Returns(realDbContext.Database);

            _userService = new UserService(
                _mockUserRepository.Object,
                _mockPostRepository.Object,
                _mockCommentRepository.Object,
                _mockDbContext.Object,
                _mockConfiguration.Object
            );
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnUserDtoWithToken()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Region = "America",
                Role = "user"
            };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.AuthenticateAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.UserName.Should().Be(user.UserName);
            result.Email.Should().Be(user.Email);
            result.Token.Should().NotBeNullOrEmpty();

            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(loginDto.Email), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidEmail_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync((User)null!);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _userService.AuthenticateAsync(loginDto));

            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(loginDto.Email), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                Region = "America",
                Role = "user"
            };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _userService.AuthenticateAsync(loginDto));

            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(loginDto.Email), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldCreateUserSuccessfully()
        {
            // Arrange
            var registerDto = new UserRegisterDto
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                Password = "password123",
                ConfirmPassword = "password123",
                Region = "Europe"
            };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(registerDto.Email.Trim().ToLowerInvariant()))
                .ReturnsAsync((User)null!);

            _mockUserRepository.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _mockUserRepository.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _userService.RegisterAsync(registerDto);

            // Assert
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(registerDto.Email.Trim().ToLowerInvariant()), Times.Once);
            _mockUserRepository.Verify(x => x.CreateUserAsync(It.Is<User>(u =>
                u.UserName == registerDto.UserName &&
                u.Email == registerDto.Email.Trim().ToLowerInvariant() &&
                u.Region == registerDto.Region &&
                u.Role == "user" &&
                u.TotalLikesReceived == 0
            )), Times.Once);
            _mockUserRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithNonMatchingPasswords_ShouldThrowArgumentException()
        {
            // Arrange
            var registerDto = new UserRegisterDto
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                Password = "password123",
                ConfirmPassword = "differentpassword",
                Region = "Europe"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.RegisterAsync(registerDto));

            exception.Message.Should().Be("Passwords do not match");
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldThrowArgumentException()
        {
            // Arrange
            var registerDto = new UserRegisterDto
            {
                UserName = "newuser",
                Email = "existing@example.com",
                Password = "password123",
                ConfirmPassword = "password123",
                Region = "Europe"
            };

            var existingUser = new User
            {
                Id = 1,
                Email = "existing@example.com"
            };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(registerDto.Email.Trim().ToLowerInvariant()))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.RegisterAsync(registerDto));

            exception.Message.Should().Be("Email is already registered");
        }

        [Fact]
        public async Task IsEmailAvailableAsync_WithAvailableEmail_ShouldReturnTrue()
        {
            // Arrange
            var email = "available@example.com";
            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync((User)null!);

            // Act
            var result = await _userService.IsEmailAvailableAsync(email);

            // Assert
            result.Should().BeTrue();
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task IsEmailAvailableAsync_WithExistingEmail_ShouldReturnFalse()
        {
            // Arrange
            var email = "existing@example.com";
            var existingUser = new User { Id = 1, Email = email };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.IsEmailAvailableAsync(email);

            // Assert
            result.Should().BeFalse();
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithValidEmail_ShouldDeleteUserAndRelatedData()
        {
            // Arrange
            var email = "user@example.com";
            var user = new User
            {
                Id = 1,
                Email = email,
                UserName = "testuser"
            };

            var posts = new List<Post>
            {
                new Post { Id = 1, UserId = user.Id },
                new Post { Id = 2, UserId = user.Id }
            };

            var comments = new List<Comment>
            {
                new Comment { Id = 1, UserId = user.Id },
                new Comment { Id = 2, UserId = user.Id }
            };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email.Trim().ToLowerInvariant()))
                .ReturnsAsync(user);

            _mockPostRepository.Setup(x => x.GetPostsByUserIdAsync(user.Id))
                .ReturnsAsync(posts);

            _mockCommentRepository.Setup(x => x.GetCommentsByUserIdAsync(user.Id))
                .ReturnsAsync(comments);

            _mockPostRepository.Setup(x => x.DeletePostAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockCommentRepository.Setup(x => x.DeleteCommentAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockUserRepository.Setup(x => x.DeleteUserAsync(user.Id))
                .ReturnsAsync(true);

            _mockPostRepository.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockCommentRepository.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockUserRepository.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _userService.DeleteAsync(email);

            // Assert
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(email.Trim().ToLowerInvariant()), Times.Once);
            _mockPostRepository.Verify(x => x.GetPostsByUserIdAsync(user.Id), Times.Once);
            _mockCommentRepository.Verify(x => x.GetCommentsByUserIdAsync(user.Id), Times.Once);
            _mockPostRepository.Verify(x => x.DeletePostAsync(1), Times.Once);
            _mockPostRepository.Verify(x => x.DeletePostAsync(2), Times.Once);
            _mockCommentRepository.Verify(x => x.DeleteCommentAsync(1), Times.Once);
            _mockCommentRepository.Verify(x => x.DeleteCommentAsync(2), Times.Once);
            _mockUserRepository.Verify(x => x.DeleteUserAsync(user.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentEmail_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email.Trim().ToLowerInvariant()))
                .ReturnsAsync((User)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _userService.DeleteAsync(email));

            exception.Message.Should().Be($"User with email {email} not found");
        }

        [Fact]
        public async Task DeleteAsync_WhenExceptionOccurs_ShouldRollbackTransaction()
        {
            // Arrange
            var email = "user@example.com";
            var user = new User { Id = 1, Email = email };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email.Trim().ToLowerInvariant()))
                .ReturnsAsync(user);

            _mockPostRepository.Setup(x => x.GetPostsByUserIdAsync(user.Id))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _userService.DeleteAsync(email));
        }

        [Fact]
        public async Task AuthenticateAsync_WithMissingJwtConfiguration_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
            };

            var mockConfigWithoutJwt = new Mock<IConfiguration>();
            mockConfigWithoutJwt.Setup(x => x["Jwt:SecretKey"]).Returns((string?)null);

            var userServiceWithoutJwt = new UserService(
                _mockUserRepository.Object,
                _mockPostRepository.Object,
                _mockCommentRepository.Object,
                _mockDbContext.Object,
                mockConfigWithoutJwt.Object
            );

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                userServiceWithoutJwt.AuthenticateAsync(loginDto));

            exception.Message.Should().Be("JWT SecretKey is not configured. Please check appsettings.json");
        }
    }
}
