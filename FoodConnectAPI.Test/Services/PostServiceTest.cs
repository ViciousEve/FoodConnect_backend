using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodConnectAPI.Data;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;
using FoodConnectAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace FoodConnectAPI.Test.Services
{
    public class PostServiceTest
    {
        private readonly Mock<IPostRepository> _mockPostRepository;
        private readonly Mock<ICommentRepository> _mockCommentRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ITagService> _mockTagService;
        private readonly Mock<AppDbContext> _mockDbContext;
        private readonly PostService _postService;

        public PostServiceTest()
        {
            _mockPostRepository = new Mock<IPostRepository>();
            _mockCommentRepository = new Mock<ICommentRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockTagService = new Mock<ITagService>();

            // Create a real in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var realDbContext = new AppDbContext(options);
            _mockDbContext = new Mock<AppDbContext>(options);

            // Setup the mock to use the real context's database
            _mockDbContext.Setup(x => x.Database).Returns(realDbContext.Database);

            _postService = new PostService(
                _mockPostRepository.Object,
                _mockCommentRepository.Object,
                _mockDbContext.Object,
                _mockUserRepository.Object,
                _mockTagService.Object
            );
        }

        [Fact]
        public async Task GetPostByIdAsync_Returns_Post()
        {
            // Arrange
            var post = new Post { Id = 1, Title = "Test" };
            _mockPostRepository.Setup(r => r.GetPostByIdAsync(1)).ReturnsAsync(post);

            // Act
            var result = await _postService.GetPostByIdAsync(1);

            // Assert
            Assert.Equal(post.Id, result.Id);
            Assert.Equal("Test", result.Title);
        }

        [Fact]
        public async Task CreatePostAsync_CreatesPost_WithTags()
        {
            // Arrange
            var userId = 1;
            var dto = new PostAddDto
            {
                Title = "Hello",
                IngredientsList = "Eggs",
                Description = "Test desc",
                Calories = 200,
                TagNames = new List<string> { "Spicy", "Healthy" }
            };

            var tags = new List<Tag> { new Tag { Id = 1, Name = "spicy" }, new Tag { Id = 2, Name = "healthy" } };

            _mockTagService.Setup(s => s.ResolveOrCreateTagsAsync(dto.TagNames)).ReturnsAsync(tags);
            _mockPostRepository.Setup(r => r.CreatePostAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
            _mockPostRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _postService.CreatePostAsync(userId, dto);

            // Assert
            _mockTagService.Verify(s => s.ResolveOrCreateTagsAsync(dto.TagNames), Times.Once);
            _mockPostRepository.Verify(r => r.CreatePostAsync(It.Is<Post>(p => p.Title == "Hello" && p.PostTags.Count == 2)), Times.Once);
            _mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreatePostAsync_ShouldCreatePostWithoutTags()
        {
            // Arrange
            var userId = 1;
            var postDto = new PostAddDto
            {
                Title = "No Tag Post",
                IngredientsList = "Salt, Water",
                Description = "Simple post",
                Calories = 10,
                TagNames = null // No tags
            };

            var mockPostRepository = new Mock<IPostRepository>();
            var mockCommentRepository = new Mock<ICommentRepository>();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockTagService = new Mock<ITagService>();
            var mockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

            var postService = new PostService(
                mockPostRepository.Object,
                mockCommentRepository.Object,
                mockDbContext.Object,
                mockUserRepository.Object,
                mockTagService.Object
            );

            mockPostRepository.Setup(repo => repo.CreatePostAsync(It.IsAny<Post>()))
                .Returns(Task.CompletedTask);
            mockPostRepository.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await postService.CreatePostAsync(userId, postDto);

            // Assert
            mockPostRepository.Verify(r => r.CreatePostAsync(It.Is<Post>(p =>
                p.Title == postDto.Title &&
                p.PostTags.Count == 0
            )), Times.Once);

            mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreatePostAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
        {
            // Arrange
            var mockPostRepository = new Mock<IPostRepository>();
            var mockCommentRepository = new Mock<ICommentRepository>();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockTagService = new Mock<ITagService>();
            var mockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

            var postService = new PostService(
                mockPostRepository.Object,
                mockCommentRepository.Object,
                mockDbContext.Object,
                mockUserRepository.Object,
                mockTagService.Object
            );

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                postService.CreatePostAsync(1, null));
        }

        [Fact]
        public async Task DeletePostAsync_DeletesPostAndComments()
        {
            // Arrange
            var postId = 1;
            var comments = new List<Comment> { new Comment { Id = 10 }, new Comment { Id = 11 } };

            var dbFacadeMock = new Mock<DatabaseFacade>(_mockDbContext.Object);
            dbFacadeMock.Setup(d => d.BeginTransactionAsync(default)).ReturnsAsync(Mock.Of<IDbContextTransaction>());
            _mockDbContext.Setup(c => c.Database).Returns(dbFacadeMock.Object);

            _mockCommentRepository.Setup(c => c.GetCommentsByPostIdAsync(postId)).ReturnsAsync(comments);
            _mockCommentRepository.Setup(c => c.DeleteCommentAsync(It.IsAny<int>())).Returns(Task.FromResult(true));
            _mockCommentRepository.Setup(c => c.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockPostRepository.Setup(p => p.DeletePostAsync(postId)).ReturnsAsync(true);
            _mockPostRepository.Setup(p => p.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _postService.DeletePostAsync(postId);

            // Assert
            Assert.True(result);
            _mockCommentRepository.Verify(c => c.DeleteCommentAsync(It.IsAny<int>()), Times.Exactly(2));
            _mockPostRepository.Verify(p => p.DeletePostAsync(postId), Times.Once);
        }
    }
}
