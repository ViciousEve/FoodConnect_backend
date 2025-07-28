using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;
using Microsoft.AspNetCore.Mvc;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace FoodConnectAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly AppDbContext _dbContext;

        public UserService(IUserRepository userRepository, IPostRepository postRepository, ICommentRepository commentRepository, AppDbContext dbContext)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _dbContext = dbContext;
        }

        public async Task DeleteAsync(string email)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Find user by email
                    var user = await _userRepository.GetUserByEmailAsync(email);
                    if (user == null)
                    {
                        throw new KeyNotFoundException($"User with email {email} not found");
                    }

                    // Delete all posts by user (and their related comments via PostService logic)
                    var posts = await _postRepository.GetPostsByUserIdAsync(user.Id);
                    foreach (var post in posts)
                    {
                        await _postRepository.DeletePostAsync(post.Id);
                    }
                    await _postRepository.SaveChangesAsync();

                    // Delete all comments by user (that are not already deleted with posts)
                    var comments = await _commentRepository.GetCommentsByUserIdAsync(user.Id);
                    foreach (var comment in comments)
                    {
                        await _commentRepository.DeleteCommentAsync(comment.Id);
                    }
                    await _commentRepository.SaveChangesAsync();

                    // Delete the user
                    await _userRepository.DeleteUserAsync(user.Id);
                    await _userRepository.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public Task<string> AuthenticateAsync([FromBody] UserLoginDto userLoginDto)
        {
            //Authentication logic using Jwt
            return Task.FromResult("");
        }

        public Task<bool> IsEmailAvailableAsync(string email)
        {
            var user = _userRepository.GetUserByEmailAsync(email);
            if(user == null)
            {
                return Task.FromResult(true); // Email is available
            }
            return Task.FromResult(false); // Email is not available
        }


        public Task RegisterAsync([FromBody] UserRegisterDto userRefisterDto)
        {
            throw new NotImplementedException();
        }
    }
}
