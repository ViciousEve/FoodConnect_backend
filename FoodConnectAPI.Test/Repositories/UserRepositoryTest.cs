using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FoodConnectAPI.Data;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Helpers;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Repositories;
using FoodConnectAPI.Test.Factories;
using Microsoft.EntityFrameworkCore;

namespace FoodConnectAPI.Test.Repositories
{
    public class UserRepositoryTest
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;
        public UserRepositoryTest()
        {
            _context = InMemoryContextFactory.Create();
            _userRepository = new UserRepository(_context);
        }

        [Fact]
        public async Task CreateUserAsync_Should_Add_User_To_Context()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@test.com", PasswordHash = HashHelper.HashPassword("Test123456!"), Region = "Asia", Role = "User" };

            // Act
            await _userRepository.CreateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            // Assert
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "testuser");
            savedUser.Should().NotBeNull();
            savedUser.Email.Should().Be("test@test.com");
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Return_True_When_User_Exists()
        {
            //Arrange
            var user = new User { UserName = "deleteMe", Email = "del@test.com", PasswordHash = HashHelper.HashPassword("Test123456!"), Region = "Asia", Role = "User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.DeleteUserAsync(user.Id);
                        await _userRepository.SaveChangesAsync();

            // Assert
            result.Should().BeTrue();
            _context.Users.Should().NotContain(u => u.Id == user.Id);
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Return_False_When_User_Does_Not_Exist()
        {
            //Act
            var result = await _userRepository.DeleteUserAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllUsersAsync_Should_Return_All_Users()
        {
            // Arrange
            _context.Users.AddRange(
                new User { UserName = "u1", Email = "u1@test.com", PasswordHash = HashHelper.HashPassword("Test123456!"), Region = "Asia", Role = "User" },
                new User { UserName = "u2", Email = "u2@test.com", PasswordHash = HashHelper.HashPassword("Test123456!"), Region = "Asia", Role = "User" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetAllUsersAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByUserNameAsync_Should_Return_Correct_User()
        {
            // Arrange
            var user = new User { UserName = "findme", Email = "findme@test.com", PasswordHash = HashHelper.HashPassword("Test123456!"), Region = "Asia", Role = "User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetByUserNameAsync("findme");

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("findme@test.com");
        }

        [Fact]
        public async Task GetUserByEmailAsync_Should_Return_Correct_User()
        {
            // Arrange
            var user = new User { UserName = "emailUser", Email = "email@test.com", PasswordHash = HashHelper.HashPassword("Test123456!"), Region = "Asia", Role = "User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetUserByEmailAsync("email@test.com");

            // Assert
            result.Should().NotBeNull();
            result.UserName.Should().Be("emailUser");
        }

        [Fact]
        public async Task GetUserByIdAsync_Should_Return_Correct_User()
        {
            // Arrange
            var user = new User { UserName = "idUser", Email = "id@test.com", PasswordHash = HashHelper.HashPassword("Test123456!"), Region = "Asia", Role = "User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetUserByIdAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.UserName.Should().Be("idUser");
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Update_User_Fields()
        {
            // Arrange
            var user = new User { UserName = "oldName", Email = "old@test.com", PasswordHash = "oldhash", Role = "User", Region = "oldRegion" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updatedUser = new User
            {
                Id = user.Id,
                UserName = "newName",
                Email = "new@test.com",
                PasswordHash = "newhash",
                Role = "Admin",
                Region = "newRegion"
            };

            // Act
            var result = await _userRepository.UpdateUserAsync(updatedUser);
            await _userRepository.SaveChangesAsync();

            // Assert
            result.UserName.Should().Be("newName");
            result.Email.Should().Be("new@test.com");
            result.Role.Should().Be("Admin");
            result.Region.Should().Be("newRegion");
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Throw_When_User_Not_Found()
        {
            // Arrange
            var fakeUser = new User { Id = 999, UserName = "doesnotexist", PasswordHash = HashHelper.HashPassword("Test123456!"), Region = "Asia", Role = "User" };

            // Act
            Func<Task> act = async () => await _userRepository.UpdateUserAsync(fakeUser);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found");
        }
        
    }
}
 