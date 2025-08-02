using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;
using Microsoft.AspNetCore.Mvc;
using FoodConnectAPI.Entities;
using FoodConnectAPI.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace FoodConnectAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IPostRepository postRepository, ICommentRepository commentRepository, AppDbContext dbContext, IConfiguration _configuration)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _dbContext = dbContext;
            _configuration = _configuration;
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

        public async Task<UserDto> AuthenticateAsync([FromBody] UserLoginDto userLoginDto)
        {
            //Authentication logic using Jwt
            var user = await _userRepository.GetUserByEmailAsync(userLoginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials"); // Invalid credentials
            }
            //Create Jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]); // Replace with your actual secret key

            if (!int.TryParse(_configuration["Jwt:ExpirationMinutes"], out int expirationMinutes))
            {
                expirationMinutes = 30; // fallback default
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),// Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            // Return user DTO with token
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Token = tokenString
            };
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


        public Task RegisterAsync([FromBody] UserRegisterDto userRegisterDto)
        {
            throw new NotImplementedException();
        }
    }
}
