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
        private readonly ILikeRepository _likeRepository;
        private readonly AppDbContext _dbContext;
        private readonly ITagService _tagService;
        private readonly IFileService _fileService;
        const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
        // Allowed extensions (lowercase)
        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

        public PostService(IPostRepository postRepository, ICommentRepository commentRepository,
            ILikeRepository likeRepository, IUserRepository userRepository, 
            ITagService tagService, IFileService fileService , AppDbContext dbContext)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _dbContext = dbContext;
            _userRepository = userRepository;
            _tagService = tagService;
            _likeRepository = likeRepository;
            _fileService = fileService;
        }

        public async Task<PostInfoDto> GetPostByIdAsync(int postId, int? currentUserId = null)
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
                Likes = post.PostLikes.Count,
                IsLikedByCurrentUser = currentUserId.HasValue && post.PostLikes.Any(l => l.UserId == currentUserId.Value)
            };
            return postInfoDto;
        }

        public async Task<IEnumerable<PostInfoDto>> GetAllPostsAsync(int? currentUserId = null)
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
                UserName = post.User.UserName,
                IsLikedByCurrentUser = currentUserId.HasValue && post.PostLikes.Any(l => l.UserId == currentUserId.Value)

            }).ToList();
            //Sort by CreatedAt newest first
            postDtos = postDtos.OrderByDescending(p => p.CreatedAt).ToList();
            return postDtos;
        }

        public async Task<IEnumerable<PostInfoDto>> GetPostsByUserIdAsync(int userId, int? currentUserId = null)
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
                Likes = post.PostLikes.Count,
                IsLikedByCurrentUser = currentUserId.HasValue && post.PostLikes.Any(l => l.UserId == currentUserId.Value)
            }).ToList();
            return postDtos;
        }

        /*Rework
         * Uses PostUpdateFormDto
         * handle changes in the image array
         * handle changes in the tag array
         * delete any orphaned tags ( tags that no other post uses)
         * wrap in a transaction to revert if any error occurs
         */
        public async Task UpdatePostAsync(int postId, PostFormDto dto)
        {
            // collect files to delete *after* commit
            var filesToDelete = new List<string>();

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                Post postToUpdate = await _postRepository.GetPostForUpdateAsync(postId);

                // Update basic fields
                postToUpdate.Title = dto.Title;
                postToUpdate.IngredientsList = dto.IngredientsList;
                postToUpdate.Description = dto.Description;
                postToUpdate.Calories = dto.Calories ?? 0;

                // Handle images (but don’t delete physical files yet)
                filesToDelete.AddRange(await UpdateImagesAsync(postToUpdate, dto));

                // Handle tags
                postToUpdate.PostTags.Clear();
                if (dto.TagNames?.Any() == true)
                {
                    var tags = await _tagService.ResolveOrCreateTagsAsync(dto.TagNames);
                    foreach (var tag in tags)
                    {
                        postToUpdate.PostTags.Add(new PostTag { Tag = tag });
                    }
                }
                await _tagService.DeleteAllOrphanTagsAsync();

                await _postRepository.SaveChangesAsync();

                // commit DB transaction
                await transaction.CommitAsync();

                // now it’s safe to delete physical files
                foreach (var filePath in filesToDelete)
                {
                    _fileService.DeleteFile(filePath);
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<bool> DeletePostAsync(int postId)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Delete all comments related to this post first
                    await _commentRepository.DeleteCommentsByPostIdAsync(postId);
                    await _commentRepository.SaveChangesAsync();
                    // Delete all likes related to this post
                    await _likeRepository.DeleteLikeByPostIdAsync(postId);
                    await _likeRepository.SaveChangesAsync();

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

        public async Task CreatePostAsync(int userId, PostFormDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var savedImageUrls = new List<string>();

            // Save uploaded files
            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {


                foreach (var file in dto.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        // Validate file size
                        if (file.Length > MaxFileSize)
                            throw new InvalidOperationException($"File {file.FileName} exceeds the maximum size of {MaxFileSize / (1024 * 1024)} MB.");

                        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

                        // Validate file extension
                        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                        {
                            throw new InvalidOperationException($"File {file.FileName} has an invalid or unsupported extension.");
                        }
                        //Vilidate MIME type for images
                        if (!file.ContentType.StartsWith("image/"))
                        {
                            throw new InvalidOperationException($"File {file.FileName} is not a valid image.");
                        }

                        var relativePath = await _fileService.SaveFileAsync(file, "Uploads");
                        savedImageUrls.Add(relativePath);
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
                UserId = userId,
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

        public async Task<bool> IsOwnerAsync(int userId, int postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            return post != null && post.UserId == userId;
        }

        private async Task<List<string>> UpdateImagesAsync(Post post, PostFormDto dto)
        {
            var savedImageUrls = new List<string>();
            var filesToDelete = new List<string>();

            // Save new uploaded files
            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                foreach (var file in dto.ImageFiles)
                {
                    if (file.Length == 0) continue;

                    if (file.Length > MaxFileSize)
                        throw new InvalidOperationException($"File {file.FileName} exceeds max size {MaxFileSize / (1024 * 1024)} MB.");

                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                        throw new InvalidOperationException($"Invalid extension: {ext}.");

                    if (!file.ContentType.StartsWith("image/"))
                        throw new InvalidOperationException($"File {file.FileName} is not a valid image.");

                    var relativePath = await _fileService.SaveFileAsync(file, "Uploads");
                    savedImageUrls.Add(relativePath);
                }
            }

            // Add provided URLs
            if (dto.ImageUrls?.Any() == true)
            {
                savedImageUrls.AddRange(dto.ImageUrls.Where(url => !string.IsNullOrWhiteSpace(url)));
            }

            // === Reconcile ===
            var existingUrls = post.Images.Select(i => i.Url).ToList();

            // Remove images that are not in dto anymore
            var toRemove = post.Images.Where(i => !savedImageUrls.Contains(i.Url)).ToList();
            foreach (var img in toRemove)
            {
                post.Images.Remove(img);

                // mark for deletion later if it’s under /Uploads
                if (img.Url.StartsWith("/Uploads"))
                {
                    filesToDelete.Add(img.Url);
                }
            }

            // Add new images
            var toAdd = savedImageUrls.Except(existingUrls).ToList();
            foreach (var newUrl in toAdd)
            {
                post.Images.Add(new Media { Url = newUrl });
            }

            return filesToDelete;
        }
    }
}
