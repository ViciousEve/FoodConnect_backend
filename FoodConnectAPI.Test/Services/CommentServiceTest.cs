using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Models;
using FoodConnectAPI.Services;
using Moq;

namespace FoodConnectAPI.Test.Services
{
    public class CommentServiceTest
    {
        private readonly Mock<ICommentRepository> _mockCommentRepository;
        private readonly Mock<IPostRepository> _mockPostRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly CommentService _commentService;

        public CommentServiceTest()
        {
            _mockCommentRepository = new Mock<ICommentRepository>();
            _mockPostRepository = new Mock<IPostRepository>();
            _mockUserRepository = new Mock<IUserRepository>();

            _commentService = new CommentService(
                _mockCommentRepository.Object,
                _mockPostRepository.Object,
                _mockUserRepository.Object);
        }

        [Fact]
        public async Task GetCommentByIdAsync_ShouldReturnComment_WhenExists()
        {
            // Arrange
            var comment = new Comment { Id = 1, Content = "Nice!", UserId = 2, PostId = 3 };
            _mockCommentRepository.Setup(x => x.GetCommentByIdAsync(1)).ReturnsAsync(comment);

            // Act
            var result = await _commentService.GetCommentByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            _mockCommentRepository.Verify(x => x.GetCommentByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetAllCommentsAsync_ShouldReturnMappedDtos_WhenCommentsExist()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new Comment
                {
                    Id = 1,
                    Content = "Great post",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                    UserId = 5,
                    User = new User { Id = 5, UserName = "alice", ProfilePictureUrl = "pic1.jpg" }
                },
                new Comment
                {
                    Id = 2,
                    Content = "Looks tasty",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                    UserId = 6,
                    User = new User { Id = 6, UserName = "bob", ProfilePictureUrl = "pic2.jpg" }
                }
            };

            _mockCommentRepository.Setup(x => x.GetAllCommentsAsync()).ReturnsAsync(comments);

            // Act
            var result = (await _commentService.GetAllCommentsAsync()).ToList();

            // Assert
            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[0].Content.Should().Be("Great post");
            result[0].UserId.Should().Be(5);
            result[0].UserName.Should().Be("alice");
            result[0].ProfilePictureUrl.Should().Be("pic1.jpg");
            result[1].UserName.Should().Be("bob");
            _mockCommentRepository.Verify(x => x.GetAllCommentsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllCommentsAsync_ShouldReturnEmpty_WhenNoComments()
        {
            // Arrange
            _mockCommentRepository.Setup(x => x.GetAllCommentsAsync()).ReturnsAsync(new List<Comment>());

            // Act
            var result = await _commentService.GetAllCommentsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllCommentsAsync_ShouldReturnEmpty_WhenRepositoryReturnsNull()
        {
            // Arrange
            _mockCommentRepository.Setup(x => x.GetAllCommentsAsync()).ReturnsAsync((IEnumerable<Comment>)null!);

            // Act
            var result = await _commentService.GetAllCommentsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCommentsByPostIdAsync_ShouldReturnMappedDtos()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new Comment
                {
                    Id = 10,
                    Content = "Comment A",
                    CreatedAt = DateTime.UtcNow,
                    UserId = 100,
                    PostId = 50,
                    User = new User { Id = 100, UserName = "userA", ProfilePictureUrl = "a.jpg" }
                }
            };

            _mockCommentRepository.Setup(x => x.GetCommentsByPostIdAsync(50)).ReturnsAsync(comments);

            // Act
            var result = (await _commentService.GetCommentsByPostIdAsync(50)).ToList();

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(10);
            result[0].UserName.Should().Be("userA");
            _mockCommentRepository.Verify(x => x.GetCommentsByPostIdAsync(50), Times.Once);
        }

        [Fact]
        public async Task GetCommentsByUserIdAsync_ShouldReturnMappedDtos()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new Comment
                {
                    Id = 11,
                    Content = "Comment B",
                    CreatedAt = DateTime.UtcNow,
                    UserId = 200,
                    PostId = 60,
                    User = new User { Id = 200, UserName = "userB", ProfilePictureUrl = "b.jpg" }
                }
            };

            _mockCommentRepository.Setup(x => x.GetCommentsByUserIdAsync(200)).ReturnsAsync(comments);

            // Act
            var result = (await _commentService.GetCommentsByUserIdAsync(200)).ToList();

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(11);
            result[0].UserName.Should().Be("userB");
            _mockCommentRepository.Verify(x => x.GetCommentsByUserIdAsync(200), Times.Once);
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldUpdateAndReturnUpdatedComment_WhenExists()
        {
            // Arrange
            var existing = new Comment { Id = 1, Content = "old", UserId = 2, PostId = 3 };
            var updateDto = new CommentUpdateDto { Content = "new content" };

            _mockCommentRepository.Setup(x => x.GetCommentByIdAsync(1)).ReturnsAsync(existing);
            _mockCommentRepository
                .Setup(x => x.UpdateCommentAsync(It.IsAny<Comment>()))
                .ReturnsAsync((Comment c) => c);
            _mockCommentRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var updated = await _commentService.UpdateCommentAsync(1, updateDto);

            // Assert
            updated.Should().NotBeNull();
            updated.Content.Should().Be("new content");
            _mockCommentRepository.Verify(x => x.UpdateCommentAsync(It.Is<Comment>(c => c.Id == 1 && c.Content == "new content")), Times.Once);
            _mockCommentRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldThrow_WhenCommentNotFound()
        {
            // Arrange
            _mockCommentRepository.Setup(x => x.GetCommentByIdAsync(999)).ReturnsAsync((Comment)null!);

            // Act & Assert
            await FluentActions.Invoking(() => _commentService.UpdateCommentAsync(999, new CommentUpdateDto { Content = "anything" }))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Comment not found*");
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldDeleteAndReturnResult()
        {
            // Arrange
            _mockCommentRepository.Setup(x => x.DeleteCommentAsync(1)).ReturnsAsync(true);
            _mockCommentRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _commentService.DeleteCommentAsync(1);

            // Assert
            result.Should().BeTrue();
            _mockCommentRepository.Verify(x => x.DeleteCommentAsync(1), Times.Once);
            _mockCommentRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrow_WhenCommentIsNull()
        {
            //Arrange
            int postId = 1;
            int userId = 1;
            //Act & Assert
            await FluentActions.Invoking(() => _commentService.CreateCommentAsync(postId, userId, null!))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrow_WhenInvalidUserId()
        {
            // Arrange
            int postId = 1;
            int userId = 0;
            var addDto = new CommentAddDto { Content = "hi" };

            // Act & Assert
            await FluentActions.Invoking(() => _commentService.CreateCommentAsync(postId, userId, addDto))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Invalid UserId*");
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrow_WhenUserNotFound()
        {
            // Arrange
            int postId = 1;
            int userId = 123;
            var addDto = new CommentAddDto { Content = "hi" };
            _mockUserRepository.Setup(x => x.GetUserByIdAsync(123)).ReturnsAsync((User)null!);

            // Act & Assert
            await FluentActions.Invoking(() => _commentService.CreateCommentAsync(postId, userId, addDto))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("User not found*");
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrow_WhenInvalidPostId()
        {
            //Arrange
            int postId = 0;
            int userId = 1;
            var addDto = new CommentAddDto { Content = "hi" };

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });

            //Act & Assert
            await FluentActions.Invoking(() => _commentService.CreateCommentAsync(postId, userId, addDto))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Invalid PostId*");
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrow_WhenPostNotFound()
        {
            //Arrange
            int postId = 2;
            int userId = 1;
            var addDto = new CommentAddDto { Content = "hi" };
            _mockUserRepository.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
            _mockPostRepository.Setup(x => x.GetPostByIdAsync(2)).ReturnsAsync((Post)null!);

            //Act & Assert
            await FluentActions.Invoking(() => _commentService.CreateCommentAsync(postId, userId, addDto))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Post not found*");
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrow_WhenContentEmpty()
        {
            //Arrange
            int postId = 2;
            int userId = 1;
            var addDto = new CommentAddDto { Content = "  " };
            _mockUserRepository.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
            _mockPostRepository.Setup(x => x.GetPostByIdAsync(2)).ReturnsAsync(new Post { Id = 2 });

            //Act & Assert
            await FluentActions.Invoking(() => _commentService.CreateCommentAsync(postId, userId, addDto))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Content cannot be empty*");
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldCreateAndSave_WhenValid()
        {
            // Arrange
            int postId = 20;
            int userId = 10;
            var addDto = new CommentAddDto { Content = "hello" };
            _mockUserRepository.Setup(x => x.GetUserByIdAsync(10)).ReturnsAsync(new User { Id = 10 });
            _mockPostRepository.Setup(x => x.GetPostByIdAsync(20)).ReturnsAsync(new Post { Id = 20 });

            Comment? created = null;
            _mockCommentRepository
                .Setup(x => x.CreateCommentAsync(It.IsAny<Comment>()))
                .Callback<Comment>(c => created = c)
                .Returns(Task.CompletedTask);
            _mockCommentRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _commentService.CreateCommentAsync(postId, userId, addDto);

            // Assert
            created.Should().NotBeNull();
            created!.Content.Should().Be("hello");
            created.UserId.Should().Be(10);
            created.PostId.Should().Be(20);
            created.CreatedAt.Should().BeAfter(DateTime.UtcNow.AddMinutes(-5));

            _mockCommentRepository.Verify(x => x.CreateCommentAsync(It.Is<Comment>(c => c.UserId == 10 && c.PostId == 20 && c.Content == "hello")), Times.Once);
            _mockCommentRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
