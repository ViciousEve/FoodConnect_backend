using System;
using System.Threading.Tasks;
using FluentAssertions;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Services;
using Moq;

namespace FoodConnectAPI.Test.Services
{
    public class LikeServiceTest
    {
        private readonly Mock<ILikeRepository> _mockLikeRepository;
        private readonly LikeService _likeService;

        public LikeServiceTest()
        {
            _mockLikeRepository = new Mock<ILikeRepository>();
            _likeService = new LikeService(_mockLikeRepository.Object);
        }

        [Fact]
        public async Task LikePostAsync_ShouldThrow_WhenUserIdInvalid()
        {
            // Act
            Func<Task> act = async () => await _likeService.LikePostAsync(0, 1);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid user ID*");
        }

        [Fact]
        public async Task LikePostAsync_ShouldThrow_WhenPostIdInvalid()
        {
            // Act
            Func<Task> act = async () => await _likeService.LikePostAsync(1, 0);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid post ID*");
        }

        [Fact]
        public async Task LikePostAsync_ShouldReturnFalse_WhenAlreadyLiked()
        {
            // Arrange
            _mockLikeRepository.Setup(r => r.UserHasLikedPostAsync(5, 10)).ReturnsAsync(true);

            // Act
            var result = await _likeService.LikePostAsync(5, 10);

            // Assert
            result.Should().BeFalse();
            _mockLikeRepository.Verify(r => r.UserHasLikedPostAsync(5, 10), Times.Once);
            _mockLikeRepository.Verify(r => r.CreateLikeAsync(It.IsAny<FoodConnectAPI.Entities.Like>()), Times.Never);
        }

        [Fact]
        public async Task LikePostAsync_ShouldCreateLike_WhenNotAlreadyLiked()
        {
            // Arrange
            _mockLikeRepository.Setup(r => r.UserHasLikedPostAsync(2, 3)).ReturnsAsync(false);
            _mockLikeRepository.Setup(r => r.CreateLikeAsync(It.IsAny<FoodConnectAPI.Entities.Like>())).Returns(Task.CompletedTask);
            _mockLikeRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _likeService.LikePostAsync(2, 3);

            // Assert
            result.Should().BeTrue();
            _mockLikeRepository.Verify(r => r.CreateLikeAsync(It.Is<FoodConnectAPI.Entities.Like>(l => l.UserId == 2 && l.PostId == 3)), Times.Once);
            _mockLikeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UnlikePostAsync_ShouldThrow_WhenUserIdInvalid()
        {
            // Act
            Func<Task> act = async () => await _likeService.UnlikePostAsync(0, 1);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid user ID*");
        }

        [Fact]
        public async Task UnlikePostAsync_ShouldThrow_WhenPostIdInvalid()
        {
            // Act
            Func<Task> act = async () => await _likeService.UnlikePostAsync(1, 0);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid post ID*");
        }

        [Fact]
        public async Task UnlikePostAsync_ShouldCallRepositoryAndReturnResult()
        {
            // Arrange
            _mockLikeRepository.Setup(r => r.DeleteLikeByUserAndPostAsync(4, 7)).ReturnsAsync(true);
            _mockLikeRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _likeService.UnlikePostAsync(4, 7);

            // Assert
            result.Should().BeTrue();
            _mockLikeRepository.Verify(r => r.DeleteLikeByUserAndPostAsync(4, 7), Times.Once);
            _mockLikeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ToggleLikeAsync_ShouldUnlike_WhenAlreadyLiked()
        {
            // Arrange
            _mockLikeRepository.Setup(r => r.UserHasLikedPostAsync(8, 9)).ReturnsAsync(true);
            _mockLikeRepository.Setup(r => r.DeleteLikeByUserAndPostAsync(8, 9)).ReturnsAsync(true);
            _mockLikeRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var nowLiked = await _likeService.ToggleLikeAsync(8, 9);

            // Assert
            nowLiked.Should().BeFalse();
            _mockLikeRepository.Verify(r => r.UserHasLikedPostAsync(8, 9), Times.Once);
            _mockLikeRepository.Verify(r => r.DeleteLikeByUserAndPostAsync(8, 9), Times.Once);
        }

        [Fact]
        public async Task ToggleLikeAsync_ShouldLike_WhenNotYetLiked()
        {
            // Arrange
            _mockLikeRepository.Setup(r => r.UserHasLikedPostAsync(12, 34)).ReturnsAsync(false);
            _mockLikeRepository.Setup(r => r.CreateLikeAsync(It.IsAny<FoodConnectAPI.Entities.Like>())).Returns(Task.CompletedTask);
            _mockLikeRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var nowLiked = await _likeService.ToggleLikeAsync(12, 34);

            // Assert
            nowLiked.Should().BeTrue();
            // Called once in ToggleLikeAsync and once inside LikePostAsync
            _mockLikeRepository.Verify(r => r.UserHasLikedPostAsync(12, 34), Times.Exactly(2));
            _mockLikeRepository.Verify(r => r.CreateLikeAsync(It.Is<FoodConnectAPI.Entities.Like>(l => l.UserId == 12 && l.PostId == 34)), Times.Once);
        }

        [Fact]
        public async Task UserHasLikedPostAsync_ShouldThrow_WhenArgsInvalid()
        {
            // Act
            Func<Task> act1 = async () => await _likeService.UserHasLikedPostAsync(0, 1);
            Func<Task> act2 = async () => await _likeService.UserHasLikedPostAsync(1, 0);

            // Assert
            await act1.Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid user ID*");
            await act2.Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid post ID*");
        }

        [Fact]
        public async Task UserHasLikedPostAsync_ShouldReturnRepositoryValue()
        {
            // Arrange
            _mockLikeRepository.Setup(r => r.UserHasLikedPostAsync(2, 5)).ReturnsAsync(true);

            // Act
            var result = await _likeService.UserHasLikedPostAsync(2, 5);

            // Assert
            result.Should().BeTrue();
            _mockLikeRepository.Verify(r => r.UserHasLikedPostAsync(2, 5), Times.Once);
        }

        [Fact]
        public async Task GetLikeCountByPostIdAsync_ShouldThrow_WhenPostIdInvalid()
        {
            // Act
            Func<Task> act = async () => await _likeService.GetLikeCountByPostIdAsync(0);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid post ID*");
        }

        [Fact]
        public async Task GetLikeCountByPostIdAsync_ShouldReturnRepositoryValue()
        {
            // Arrange
            _mockLikeRepository.Setup(r => r.GetLikeCountByPostIdAsync(77)).ReturnsAsync(3);

            // Act
            var count = await _likeService.GetLikeCountByPostIdAsync(77);

            // Assert
            count.Should().Be(3);
            _mockLikeRepository.Verify(r => r.GetLikeCountByPostIdAsync(77), Times.Once);
        }
    }
}


