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

        public async Task<Post> GetPostByIdAsync(int postId)
        {
            return await _postRepository.GetPostByIdAsync(postId);
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _postRepository.GetAllPostsAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId)
        {
            return await _postRepository.GetPostsByUserIdAsync(userId);
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

        public async Task CreatePostAsync(int userId, PostAddDto postAddDto)
        {
            if (postAddDto == null)
                throw new ArgumentNullException(nameof(postAddDto));

            var post = new Post
            {
                Title = postAddDto.Title,
                IngredientsList = postAddDto.IngredientsList,
                Description = postAddDto.Description,
                Calories = postAddDto.Calories ?? 0,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            // Add tags if provided
            if (postAddDto.TagNames?.Any() == true)
            {
                var tags = await _tagService.ResolveOrCreateTagsAsync(postAddDto.TagNames);
                foreach (var tag in tags)
                {
                    post.PostTags.Add(new PostTag { Tag = tag });
                }
            }

            // Add images if provided (Todo later)
            //if (postAddDto.ImageUrls?.Any() == true)
            //{
            //    post.Images = new List<Media>();
            //    foreach (var image in postAddDto.ImageUrls)
            //    {
            //        post.Images.Add(new Media { Url = image.Url, Type = image.Type });
            //    }
            //}
            await _postRepository.CreatePostAsync(post);
            await _postRepository.SaveChangesAsync();
        }

    }
}
