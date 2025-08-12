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
                    PostTags = new List<PostTag> { new PostTag { Tag = new Tag { Name = "Tag1" } } },
                    Images = new List<Media> { new Media { Url = "url1" } },
                    PostLikes = new List<Like> { new Like() }
                }
            };
            _mockPostRepository.Setup(r => r.GetAllPostsAsync()).ReturnsAsync(posts);

            // Act
            var result = await _postService.GetAllPostsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Post 1");
            result.First().TagNames.Should().Contain("Tag1");
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
            await FluentActions.Invoking(() => postService.CreatePostAsync(1, null))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreatePostAsync_CreatesPostTags_WithTagId()
        {
            // Arrange
            var userId = 1;
            var tag1 = new Tag { Id = 10, Name = "Spicy" };
            var tag2 = new Tag { Id = 20, Name = "Healthy" };
            var tagList = new List<Tag> { tag1, tag2 };

            var postDto = new PostAddDto
            {
                Title = "Test Post",
                IngredientsList = "Eggs",
                Description = "Test desc",
                Calories = 100,
                TagNames = new List<string> { "Spicy", "Healthy" }
            };

            _mockTagService.Setup(s => s.ResolveOrCreateTagsAsync(postDto.TagNames))
                .ReturnsAsync(tagList);

            Post capturedPost = null;
            _mockPostRepository.Setup(r => r.CreatePostAsync(It.IsAny<Post>()))
                .Callback<Post>(p => capturedPost = p)
                .Returns(Task.CompletedTask);

            _mockPostRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _postService.CreatePostAsync(userId, postDto);

            // Assert
            capturedPost.Should().NotBeNull();
            capturedPost.PostTags.Count.Should().Be(2);
            capturedPost.PostTags.Should().Contain(pt => pt.TagId == tag1.Id || (pt.Tag != null && pt.Tag.Id == tag1.Id));
            capturedPost.PostTags.Should().Contain(pt => pt.TagId == tag2.Id || (pt.Tag != null && pt.Tag.Id == tag2.Id));
            foreach (var postTag in capturedPost.PostTags)
            {
                (postTag.TagId > 0 || (postTag.Tag != null && postTag.Tag.Id > 0)).Should().BeTrue();
                postTag.PostId.Should().Be(0); // PostId is 0 since not saved to DB
            }
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
            result.Should().BeTrue();
            _mockCommentRepository.Verify(c => c.DeleteCommentAsync(It.IsAny<int>()), Times.Exactly(2));
            _mockPostRepository.Verify(p => p.DeletePostAsync(postId), Times.Once);
        }
    }
}
