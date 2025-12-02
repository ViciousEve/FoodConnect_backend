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
        private readonly IFileService _fileService;

        const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
        // Allowed extensions (lowercase)
        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

        public UserService(IUserRepository userRepository, IPostRepository postRepository,
            ICommentRepository commentRepository, AppDbContext dbContext,
            IConfiguration configuration, IFileService fileService)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _dbContext = dbContext;
            _configuration = configuration;
            _fileService = fileService;
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
                    if (posts == null)
                    {
                        posts = new List<Post>();
                    }
                    foreach (var post in posts)
                    {
                        await _postRepository.DeletePostAsync(post.Id);
                    }
                    await _postRepository.SaveChangesAsync();

                    // Delete all comments by user (that are not already deleted with posts)
                    var comments = await _commentRepository.GetCommentsByUserIdAsync(user.Id);
                    if (comments == null)
                    {
                        comments = new List<Comment>();
                    }
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
                return null; // Invalid credentials
            }
            //Create Jwt token
            var tokenString = GenerateJwtToken(user);

            // Return user DTO with token
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.DisplayEmail != null ? user.DisplayEmail : user.Email,
                Region = user.Region,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Token = tokenString
            };
        }

        private string GenerateJwtToken(User user)
        {
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
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes), // Token expiration time
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return true; // Email is available
            }
            return false; // Email is not available
        }


        public async Task<UserDto> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            //Validate that password match
            if (userRegisterDto.Password != userRegisterDto.ConfirmPassword)
            {
                throw new ArgumentException("Passwords do not match");
            }
            //check if email is available
            string email = userRegisterDto.Email.Trim().ToLowerInvariant();
            string normalizedEmail = email;
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
                DisplayEmail = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password),
                Region = userRegisterDto.Region,
                Role = "user", // Default role for new users
                TotalLikesReceived = 0
            };

            //Add user to repository
            await _userRepository.CreateUserAsync(newUser);
            await _userRepository.SaveChangesAsync();

            return new UserDto
            {
                Id = newUser.Id,
                UserName = newUser.UserName,
                Email = newUser.DisplayEmail != null ? newUser.DisplayEmail : newUser.Email,
                Region = newUser.Region
            };
        }

        public async Task UpdateProfilePicture(int userId, IFormFile profilePicture)
        {
            var user = await _userRepository.GetUserForUpdateAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Validate file size
            if (profilePicture.Length > MaxFileSize)
                throw new InvalidOperationException($"File {profilePicture.FileName} exceeds the maximum size of {MaxFileSize / (1024 * 1024)} MB.");

            var ext = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();

            // Validate file extension
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            {
                throw new InvalidOperationException($"File {profilePicture.FileName} has an invalid or unsupported extension.");
            }
            //Vilidate MIME type for images
            if (!profilePicture.ContentType.StartsWith("image/"))
            {
                throw new InvalidOperationException($"File {profilePicture.FileName} is not a valid image.");
            }

            var relativePath = await _fileService.SaveFileAsync(profilePicture, "Uploads");

            // Update user's profile picture URL
            user.ProfilePictureUrl = relativePath;
            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<UserDto> UpdateProfile(int userId, UserUpdateDto userUpdateDto)
        {
            var user = await _userRepository.GetUserForUpdateAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }
            // check email availability  if changed
            string normalizedEmail = userUpdateDto.Email.Trim().ToLowerInvariant();
            if (!await IsEmailAvailableAsync(userUpdateDto.Email) && user.Email != normalizedEmail)
            {
                throw new ArgumentException("Email is already registered");
            }

            user.Email = normalizedEmail;
            user.DisplayEmail = userUpdateDto.Email;

            // Update other fields if provided
            if (!string.IsNullOrWhiteSpace(userUpdateDto.UserName))
            {
                user.UserName = userUpdateDto.UserName;
            }
            if (!string.IsNullOrWhiteSpace(userUpdateDto.Region))
            {
                user.Region = userUpdateDto.Region;
            }
            if (!string.IsNullOrWhiteSpace(userUpdateDto.Password))
            {
                if (userUpdateDto.Password != userUpdateDto.ConfirmPassword)
                {
                    throw new ArgumentException("Passwords do not match");
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userUpdateDto.Password);
            }
            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            //create new token and return updated user dto
            var tokenString = GenerateJwtToken(user);
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.DisplayEmail != null ? user.DisplayEmail : user.Email,
                Region = user.Region,
                Token = tokenString //Could cause issues if old token is still valid 
            };
        }

    }
}
