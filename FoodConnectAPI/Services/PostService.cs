using FoodConnectAPI.Entities;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace FoodConnectAPI.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly AppDbContext _dbContext;

        public PostService(IPostRepository postRepository, ICommentRepository commentRepository, AppDbContext dbContext)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _dbContext = dbContext;
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

        public async Task CreatePostAsync(Post post)
        {
            await _postRepository.CreatePostAsync(post);
            await _postRepository.SaveChangesAsync();
        }
    }
}
