using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodConnectAPI.Data;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit.Sdk;
using FluentAssertions;

namespace FoodConnectAPI.Test.Services
{
    public class TagServiceTest
    {
        private readonly Mock<ITagRepository> _mockTagRepository;
        private readonly Mock<IPostTagRepository> _mockPostTagRepository;
        //private readonly Mock<AppDbContext> _mockDbContext;
        private readonly TagService _tagService;
        public TagServiceTest()
        {
            _mockTagRepository = new Mock<ITagRepository>();
            _mockPostTagRepository = new Mock<IPostTagRepository>();

            //_mockDbContext = new Mock<AppDbContext>();

            _tagService = new TagService(
                _mockTagRepository.Object, 
                _mockPostTagRepository.Object);
        }

        [Fact]
        public async Task ResolveOrCreateTagsAsync_ShouldReturnExistingTags()
        {
            // Arrange
            var tagNames = new List<string> { "Spicy", "Sweet" };
            var existingTags = new List<Tag>
            {
                new Tag { Id = 1, Name = "spicy" },
                new Tag { Id = 2, Name = "sweet" }
            };

            _mockTagRepository
                .Setup(repo => repo.GetTagsByNamesAsync(It.Is<List<string>>(l =>
                    l.SequenceEqual(new[] { "spicy", "sweet" }))))
                .ReturnsAsync(existingTags);

            // Act
            var result = await _tagService.ResolveOrCreateTagsAsync(tagNames);

            // Assert
            result.Count.Should().Be(2);
            result.Should().OnlyContain(r => new[] { "spicy", "sweet" }.Contains(r.Name));

            _mockTagRepository.Verify(repo => repo.CreateRangeAsync(It.IsAny<List<Tag>>()), Times.Never);
            _mockTagRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ResolveOrCreateTagsAsync_ShouldCreateNewTags()
        {
            // Arrange
            var tagNames = new List<string> { "NewTag", "AnotherTag" };
            var normalized = new List<string> { "newtag", "anothertag" };

            _mockTagRepository
                .Setup(repo => repo.GetTagsByNamesAsync(normalized))
                .ReturnsAsync(new List<Tag>()); // No existing tags

            _mockTagRepository
                .Setup(repo => repo.CreateRangeAsync(It.IsAny<List<Tag>>()))
                .Returns(Task.CompletedTask);

            _mockTagRepository
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _tagService.ResolveOrCreateTagsAsync(tagNames);

            // Assert
            result.Count.Should().Be(2);
            result.Should().Contain(r => r.Name == "newtag");
            result.Should().Contain(r => r.Name == "anothertag");

            _mockTagRepository.Verify(repo => repo.CreateRangeAsync(
                It.Is<List<Tag>>(l => l.Count == 2 && l.All(t => normalized.Contains(t.Name)))
            ), Times.Once);

            _mockTagRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ResolveOrCreateTagsAsync_ShouldNormalizeTagNames()
        {
            // Arrange
            var input = new List<string> { "  Foo  ", "  BAR " };
            var normalized = new List<string> { "foo", "bar" };

            _mockTagRepository
                .Setup(repo => repo.GetTagsByNamesAsync(normalized))
                .ReturnsAsync(new List<Tag>()); // Assume none exist

            _mockTagRepository
                .Setup(repo => repo.CreateRangeAsync(It.Is<List<Tag>>(tags =>
                    tags.Any(t => t.Name == "foo") && tags.Any(t => t.Name == "bar")
                )))
                .Returns(Task.CompletedTask);

            _mockTagRepository
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _tagService.ResolveOrCreateTagsAsync(input);

            // Assert
            result.Count.Should().Be(2);
            result.Should().Contain(t => t.Name == "foo");
            result.Should().Contain(t => t.Name == "bar");

            _mockTagRepository.Verify(repo => repo.GetTagsByNamesAsync(normalized), Times.Once);
            _mockTagRepository.Verify(repo => repo.CreateRangeAsync(It.IsAny<List<Tag>>()), Times.Once);
            _mockTagRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteOrphanTagAsync_ShouldReturnFalse_WhenTagHasPosts()
        {
            // Arrange
            var tag = new Tag { Id = 5, Name = "unusedtag" };

            _mockTagRepository.Setup(r => r.GetTagByIdAsync(5)).ReturnsAsync(tag);
            _mockPostTagRepository.Setup(r => r.ExistsWithTagIdAsync(5)).ReturnsAsync(true);

            // Act
            var result = await _tagService.DeleteOrphanTagAsync(5);

            // Assert
            result.Should().BeFalse();
            _mockTagRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteOrphanTagAsync_ShouldDelete_WhenUnused()
        {
            // Arrange
            var tag = new Tag { Id = 7, Name = "unusedtag" };

            _mockTagRepository.Setup(r => r.GetTagByIdAsync(7)).ReturnsAsync(tag);
            _mockPostTagRepository.Setup(r => r.ExistsWithTagIdAsync(7)).ReturnsAsync(false);

            _mockTagRepository.Setup(r => r.DeleteAsync(7)).Returns(Task.FromResult(true));
            _mockTagRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _tagService.DeleteOrphanTagAsync(7);

            // Assert
            result.Should().BeTrue();
            _mockTagRepository.Verify(r => r.DeleteAsync(7), Times.Once);
            _mockTagRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteOrphanTagByName_ShouldReturnFalse_WhenTagHasPosts()
        {
            // Arrange
            var tag = new Tag { Id = 5, Name = "test" };
            string tagName = "Test";

            _mockTagRepository.Setup(r => r.GetTagByNameAsync("test")).ReturnsAsync(tag);
            _mockPostTagRepository.Setup(r => r.ExistsWithTagIdAsync(5)).ReturnsAsync(true);

            // Act
            var result = await _tagService.DeleteOrphanTagByNameAsync(tagName);

            // Assert
            result.Should().BeFalse();
            _mockTagRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteOrphanTagByName_ShouldDelete_WhenUnused()
        {
            // Arrange
            var tag = new Tag { Id = 7, Name = "unusedtag" };
            string tagName = "unusedtag";

            _mockTagRepository.Setup(r => r.GetTagByNameAsync("unusedtag")).ReturnsAsync(tag);
            _mockPostTagRepository.Setup(r => r.ExistsWithTagIdAsync(7)).ReturnsAsync(false);

            _mockTagRepository.Setup(r => r.DeleteAsync(7)).Returns(Task.FromResult(true));
            _mockTagRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _tagService.DeleteOrphanTagByNameAsync(tagName);

            // Assert
            result.Should().BeTrue();
            _mockTagRepository.Verify(r => r.DeleteAsync(7), Times.Once);
            _mockTagRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
