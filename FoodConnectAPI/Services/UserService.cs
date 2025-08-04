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

        public UserService(IUserRepository userRepository, IPostRepository postRepository, ICommentRepository commentRepository, AppDbContext dbContext, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task DeleteAsync(string email)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Find user by email
                    string normalizedEmail = email.Trim().ToLowerInvariant();
                    var user = await _userRepository.GetUserByEmailAsync(normalizedEmail);
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

        public async Task<UserDto> AuthenticateAsync(UserLoginDto userLoginDto)
        {
            //Authentication logic using Jwt
            var user = await _userRepository.GetUserByEmailAsync(userLoginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials"); // Invalid credentials
            }
            
            //Create Jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Get JWT configuration with fallback
            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured. Please check appsettings.json");
            }
            
            var key = Encoding.ASCII.GetBytes(secretKey);

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

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if(user == null)
            {
                return true; // Email is available
            }
            return false; // Email is not available
        }


        public async Task RegisterAsync(UserRegisterDto userRegisterDto)
        {
            //Validate that password match
            if (userRegisterDto.Password != userRegisterDto.ConfirmPassword)
            {
                throw new ArgumentException("Passwords do not match");
            }
            //check if email is available
            string normalizedEmail = userRegisterDto.Email.Trim().ToLowerInvariant();
            bool isEmailAvailable = await IsEmailAvailableAsync(normalizedEmail);
            if (!isEmailAvailable)
            {
                throw new ArgumentException("Email is already registered");
            }

            //Create user entity
            var newUser = new User
            {
                UserName = userRegisterDto.UserName,
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password),
                Region = userRegisterDto.Region,
                Role = "user", // Default role for new users
                TotalLikesReceived = 0
            };

            //Add user to repository
            await _userRepository.CreateUserAsync(newUser);
            await _userRepository.SaveChangesAsync();
        }
    }
}
