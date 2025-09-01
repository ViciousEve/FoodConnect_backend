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
using FluentAssertions;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Http;

namespace FoodConnectAPI.Test.Services
{
    public class PostServiceTest
    {
        private readonly Mock<IPostRepository> _mockPostRepository;
        private readonly Mock<ICommentRepository> _mockCommentRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ITagService> _mockTagService;
        private readonly Mock<ILikeRepository> _mockLikeRepository;
        private readonly Mock<AppDbContext> _mockDbContext;
        private readonly PostService _postService;

        public PostServiceTest()
        {
            _mockPostRepository = new Mock<IPostRepository>();
            _mockCommentRepository = new Mock<ICommentRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockTagService = new Mock<ITagService>();
            _mockLikeRepository = new Mock<ILikeRepository>();

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
                _mockLikeRepository.Object,
                _mockUserRepository.Object,
                _mockTagService.Object,
                _mockDbContext.Object
            );
        }

        [Fact]
        public async Task GetPostByIdAsync_ShouldThrowArgumentException_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = 0;

            // Act
            Func<Task> act = async () => await _postService.GetPostByIdAsync(invalidId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Invalid post ID*");
        }

        [Fact]
        public async Task GetPostByIdAsync_ShouldReturnNull_WhenPostNotFound()
        {
            // Arrange
            var postId = 1;
            _mockPostRepository.Setup(r => r.GetPostByIdAsync(postId))
                .ReturnsAsync((Post)null);

            // Act
            var result = await _postService.GetPostByIdAsync(postId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetPostByIdAsync_ShouldReturnDto_WhenPostExists()
        {
            // Arrange
            var postId = 1;
            var post = new Post
            {
                Id = postId,
                Title = "Test",
                IngredientsList = "Eggs",
                Description = "Desc",
                Calories = 100,
                CreatedAt = DateTime.UtcNow,
                UserId = 2,
                User = new User { Id = 1, UserName = "TestUser" },
                PostTags = new List<PostTag> { new PostTag { Tag = new Tag { Name = "Tag1" } } },
                Images = new List<Media> { new Media { Url = "url1" } },
                PostLikes = new List<Like> { new Like() },
            };
            _mockPostRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync(post);

            // Act
            var result = await _postService.GetPostByIdAsync(postId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(postId);
            result.TagNames.Should().Contain("Tag1");
            result.ImagesUrl.Should().Contain("url1");
            result.Likes.Should().Be(1);
        }

        [Fact]
        public async Task GetAllPostsAsync_ShouldReturnEmptyList_WhenNoPostsExist()
        {
            // Arrange
            _mockPostRepository.Setup(r => r.GetAllPostsAsync())
                .ReturnsAsync(new List<Post>());

            // Act
            var result = await _postService.GetAllPostsAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllPostsAsync_ShouldReturnDtos_WhenPostsExist()
        {
            // Arrange
            var posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = "Post 1",
                    IngredientsList = "Eggs",
                    Description = "Desc",
                    Calories = 100,
                    CreatedAt = DateTime.UtcNow,
                    UserId = 1,
                    User = new User { Id = 1, UserName = "TestUser" },
                    PostTags = new List<PostTag> { new PostTag { Tag = new Tag { Name = "Tag1" } } },
                    Images = new List<Media> { new Media { Url = "url1" } },
                    PostLikes = new List<Like> { new Like { UserId = 5, PostId = 1 } }
                }
            };
            _mockPostRepository.Setup(r => r.GetAllPostsAsync()).ReturnsAsync(posts);

            // Act
            var result = await _postService.GetAllPostsAsync(5);

            // Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Post 1");
            result.First().TagNames.Should().Contain("Tag1");
            result.First().IsLikedByCurrentUser.Should().BeTrue();
        }

        [Fact]
        public async Task GetPostByIdAsync_ShouldSetIsLikedByCurrentUser_WhenUserLiked()
        {
            // Arrange
            var postId = 2;
            var currentUserId = 7;
            var post = new Post
            {
                Id = postId,
                Title = "Post 2",
                IngredientsList = "Milk",
                Description = "Desc2",
                Calories = 50,
                CreatedAt = DateTime.UtcNow,
                UserId = 9,
                User = new User { Id = 9, UserName = "Owner" },
                PostTags = new List<PostTag>(),
                Images = new List<Media>(),
                PostLikes = new List<Like> { new Like { UserId = currentUserId, PostId = postId } }
            };
            _mockPostRepository.Setup(r => r.GetPostByIdAsync(postId)).ReturnsAsync(post);

            // Act
            var result = await _postService.GetPostByIdAsync(postId, currentUserId);

            // Assert
            result.Should().NotBeNull();
            result.IsLikedByCurrentUser.Should().BeTrue();
        }

        [Fact]
        public async Task GetPostsByUserIdAsync_ShouldThrowArgumentException_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = 0;

            // Act
            Func<Task> act = async () => await _postService.GetPostsByUserIdAsync(invalidId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Invalid user ID*");
        }

        [Fact]
        public async Task GetPostsByUserIdAsync_ShouldReturnEmptyList_WhenNoPostsExist()
        {
            // Arrange
            _mockPostRepository.Setup(r => r.GetPostsByUserIdAsync(1))
                .ReturnsAsync(new List<Post>());

            // Act
            var result = await _postService.GetPostsByUserIdAsync(1);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPostsByUserIdAsync_ShouldReturnDtos_WhenPostsExist()
        {
            // Arrange
            var posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = "User Post",
                    IngredientsList = "Eggs",
                    Description = "Desc",
                    Calories = 100,
                    CreatedAt = DateTime.UtcNow,
                    UserId = 1,
                    PostTags = new List<PostTag> { new PostTag { Tag = new Tag { Name = "Tag1" } } },
                    Images = new List<Media> { new Media { Url = "url1" } },
                    PostLikes = new List<Like> { new Like() }
                }
            };
            _mockPostRepository.Setup(r => r.GetPostsByUserIdAsync(1)).ReturnsAsync(posts);

            // Act
            var result = await _postService.GetPostsByUserIdAsync(1);

            // Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be("User Post");
            result.First().TagNames.Should().Contain("Tag1");
        }

        //To be reworked: create posts test goes here

        [Fact]
        public async Task CreatePostAsync_WithPostFormDto_CreatesPostWithTagsAndImages()
        {
            // Arrange
            var userId = 1;
            var dto = new PostFormDto
            {
                Title = "Test Post with Images",
                IngredientsList = "Eggs, Milk, Flour",
                Description = "Test description with images",
                Calories = 250,
                TagNames = new List<string> { "Breakfast", "Healthy" },
                ImageUrls = new List<string> { "https://example.com/image1.jpg", "https://example.com/image2.jpg" },
                ImageFiles = new List<IFormFile>() // Empty for this test
            };

            var tags = new List<Tag> 
            { 
                new Tag { Id = 1, Name = "Breakfast" }, 
                new Tag { Id = 2, Name = "Healthy" } 
            };

            _mockTagService.Setup(s => s.ResolveOrCreateTagsAsync(dto.TagNames)).ReturnsAsync(tags);
            _mockPostRepository.Setup(r => r.CreatePostAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
            _mockPostRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _postService.CreatePostAsync(userId, dto);

            // Assert
            _mockTagService.Verify(s => s.ResolveOrCreateTagsAsync(dto.TagNames), Times.Once);
            _mockPostRepository.Verify(r => r.CreatePostAsync(It.Is<Post>(p => 
                p.Title == dto.Title && 
                p.IngredientsList == dto.IngredientsList &&
                p.Description == dto.Description &&
                p.Calories == dto.Calories &&
                p.UserId == userId &&
                p.PostTags.Count == 2 &&
                p.Images.Count == 2)), Times.Once);
            _mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreatePostAsync_WithPostFormDto_CreatesPostWithoutTags()
        {
            // Arrange
            var userId = 2;
            var dto = new PostFormDto
            {
                Title = "Post without tags",
                IngredientsList = "Salt, Water",
                Description = "Simple post without tags",
                Calories = 10,
                TagNames = null,
                ImageUrls = new List<string>(),
                ImageFiles = new List<IFormFile>()
            };

            _mockPostRepository.Setup(r => r.CreatePostAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
            _mockPostRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _postService.CreatePostAsync(userId, dto);

            // Assert
            _mockPostRepository.Verify(r => r.CreatePostAsync(It.Is<Post>(p =>
                p.Title == dto.Title &&
                p.UserId == userId &&
                p.PostTags.Count == 0 &&
                p.Images.Count == 0)), Times.Once);
            _mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreatePostAsync_WithPostFormDto_CreatesPostWithoutImages()
        {
            // Arrange
            var userId = 3;
            var dto = new PostFormDto
            {
                Title = "Post without images",
                IngredientsList = "Rice, Vegetables",
                Description = "Post with no images",
                Calories = 180,
                TagNames = new List<string> { "Vegetarian" },
                ImageUrls = new List<string>(),
                ImageFiles = new List<IFormFile>()
            };

            var tags = new List<Tag> { new Tag { Id = 3, Name = "Vegetarian" } };

            _mockTagService.Setup(s => s.ResolveOrCreateTagsAsync(dto.TagNames)).ReturnsAsync(tags);
            _mockPostRepository.Setup(r => r.CreatePostAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
            _mockPostRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _postService.CreatePostAsync(userId, dto);

            // Assert
            _mockPostRepository.Verify(r => r.CreatePostAsync(It.Is<Post>(p =>
                p.Title == dto.Title &&
                p.UserId == userId &&
                p.Images.Count == 0)), Times.Once);
            _mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreatePostAsync_WithPostFormDto_ShouldThrowArgumentNullException_WhenDtoIsNull()
        {
            // Arrange
            var userId = 1;

            // Act & Assert
            await FluentActions.Invoking(() => _postService.CreatePostAsync(userId, (PostFormDto)null))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreatePostAsync_WithPostFormDto_CreatesPostWithOnlyImageUrls()
        {
            // Arrange
            var userId = 4;
            var dto = new PostFormDto
            {
                Title = "Post with image URLs only",
                IngredientsList = "Pasta, Sauce",
                Description = "Post with external image URLs",
                Calories = 320,
                TagNames = new List<string>(),
                ImageUrls = new List<string> { "https://example.com/pasta.jpg", "https://example.com/sauce.jpg" },
                ImageFiles = new List<IFormFile>()
            };

            _mockPostRepository.Setup(r => r.CreatePostAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
            _mockPostRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _postService.CreatePostAsync(userId, dto);

            // Assert
            _mockPostRepository.Verify(r => r.CreatePostAsync(It.Is<Post>(p =>
                p.Title == dto.Title &&
                p.UserId == userId &&
                p.Images.Count == 2 &&
                p.Images.Any(i => i.Url == "https://example.com/pasta.jpg") &&
                p.Images.Any(i => i.Url == "https://example.com/sauce.jpg"))), Times.Once);
            _mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreatePostAsync_WithPostFormDto_CreatesPostWithEmptyLists()
        {
            // Arrange
            var userId = 5;
            var dto = new PostFormDto
            {
                Title = "Post with empty lists",
                IngredientsList = "Basic ingredients",
                Description = "Post with empty tag and image lists",
                Calories = null,
                TagNames = new List<string>(),
                ImageUrls = new List<string>(),
                ImageFiles = new List<IFormFile>()
            };

            _mockPostRepository.Setup(r => r.CreatePostAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
            _mockPostRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _postService.CreatePostAsync(userId, dto);

            // Assert
            _mockPostRepository.Verify(r => r.CreatePostAsync(It.Is<Post>(p =>
                p.Title == dto.Title &&
                p.UserId == userId &&
                p.PostTags.Count == 0 &&
                p.Images.Count == 0 &&
                p.Calories == 0)), Times.Once);
            _mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
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
            _mockCommentRepository.Setup(c => c.DeleteCommentsByPostIdAsync(postId)).ReturnsAsync(true);
            _mockCommentRepository.Setup(c => c.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockPostRepository.Setup(p => p.DeletePostAsync(postId)).ReturnsAsync(true);
            _mockPostRepository.Setup(p => p.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _postService.DeletePostAsync(postId);

            // Assert
            result.Should().BeTrue();
            _mockCommentRepository.Verify(c => c.DeleteCommentsByPostIdAsync(postId), Times.Once);
            _mockPostRepository.Verify(p => p.DeletePostAsync(postId), Times.Once);
        }
    }
}
