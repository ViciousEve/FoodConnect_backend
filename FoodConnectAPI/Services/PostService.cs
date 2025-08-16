using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Data;
using Microsoft.EntityFrameworkCore.Storage;
using FoodConnectAPI.Models;

namespace FoodConnectAPI.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _dbContext;
        private readonly ITagService _tagService;

        public PostService(IPostRepository postRepository, ICommentRepository commentRepository, AppDbContext dbContext, IUserRepository userRepository, ITagService tagService)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _dbContext = dbContext;
            _userRepository = userRepository;
            _tagService = tagService;
        }

        public async Task<PostInfoDto> GetPostByIdAsync(int postId)
        {
            if(postId <= 0)
                throw new ArgumentException("Invalid post ID", nameof(postId));

            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
                return null;
            // Map Post to PostInfoDto
            var postInfoDto = new PostInfoDto
            {
                Id = post.Id,
                Title = post.Title,
                IngredientsList = post.IngredientsList,
                Description = post.Description,
                Calories = post.Calories,
                CreatedAt = post.CreatedAt,
                UserId = post.UserId,
                TagNames = post.PostTags.Select(pt => pt.Tag.Name).ToList(),
                ImagesUrl = post.Images?.Select(i => i.Url).ToList() ?? new List<string>(),
                Likes = post.PostLikes.Count
            };
            return postInfoDto;
        }

        public async Task<IEnumerable<PostInfoDto>> GetAllPostsAsync()
        {
            var posts = await _postRepository.GetAllPostsAsync();
            if (posts == null || !posts.Any())
                return new List<PostInfoDto>();
            // Map List<Post> to List<PostInfoDto>
            var postDtos = posts.Select(post => new PostInfoDto
            {
                Id = post.Id,
                Title = post.Title,
                IngredientsList = post.IngredientsList,
                Description = post.Description,
                Calories = post.Calories,
                CreatedAt = post.CreatedAt,
                UserId = post.UserId,
                TagNames = post.PostTags.Select(pt => pt.Tag.Name).ToList(),
                ImagesUrl = post.Images?.Select(i => i.Url).ToList() ?? new List<string>(),
                Likes = post.PostLikes.Count,
                UserName = post.User.UserName

            }).ToList();
            //Sort by CreatedAt newest first
            postDtos = postDtos.OrderByDescending(p => p.CreatedAt).ToList();
            return postDtos;
        }

        public async Task<IEnumerable<PostInfoDto>> GetPostsByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            var posts = await _postRepository.GetPostsByUserIdAsync(userId);

            if (posts == null || !posts.Any())
                return new List<PostInfoDto>();
            // Map List<Post> to List<PostInfoDto>
            var postDtos = posts.Select(post => new PostInfoDto
            {
                Id = post.Id,
                Title = post.Title,
                IngredientsList = post.IngredientsList,
                Description = post.Description,
                Calories = post.Calories,
                CreatedAt = post.CreatedAt,
                UserId = post.UserId,
                TagNames = post.PostTags.Select(pt => pt.Tag.Name).ToList(),
                ImagesUrl = post.Images?.Select(i => i.Url).ToList() ?? new List<string>(),
                Likes = post.Likes
            }).ToList();
            return postDtos;
        }

        public async Task<Post> UpdatePostAsync(Post post)
        {
            var updated = await _postRepository.UpdatePostAsync(post);
            await _postRepository.SaveChangesAsync();
            return updated;
        }

        public async Task<bool> DeletePostAsync(int postId)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Delete all comments related to this post first
                    var comments = await _commentRepository.GetCommentsByPostIdAsync(postId);
                    foreach (var comment in comments)
                    {
                        await _commentRepository.DeleteCommentAsync(comment.Id);
                    }
                    await _commentRepository.SaveChangesAsync();

                    var deleted = await _postRepository.DeletePostAsync(postId);
                    await _postRepository.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return deleted;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task CreatePostAsync(PostFormDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var savedImageUrls = new List<string>();

            // Save uploaded files
            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var file in dto.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        var ext = Path.GetExtension(file.FileName);
                        var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        savedImageUrls.Add($"/Uploads/{uniqueFileName}");
                    }
                }
            }

            // Add provided URLs
            if (dto.ImageUrls?.Any() == true)
            {
                savedImageUrls.AddRange(dto.ImageUrls.Where(url => !string.IsNullOrWhiteSpace(url)));
            }

            var post = new Post
            {
                Title = dto.Title,
                IngredientsList = dto.IngredientsList,
                Description = dto.Description,
                Calories = dto.Calories ?? 0,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow,
                PostTags = new List<PostTag>(),
                Images = new List<Media>()
            };

            // Add tags
            if (dto.TagNames?.Any() == true)
            {
                var tags = await _tagService.ResolveOrCreateTagsAsync(dto.TagNames);
                foreach (var tag in tags)
                {
                    post.PostTags.Add(new PostTag { Tag = tag });
                }
            }

            // Add images
            foreach (var imageUrl in savedImageUrls)
            {
                post.Images.Add(new Media { Url = imageUrl });
            }

            await _postRepository.CreatePostAsync(post);
            await _postRepository.SaveChangesAsync();
        }

    }
}
