using FoodConnectAPI.Data;
using FoodConnectAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodConnectAPI.Interfaces.Repositories;

namespace FoodConnectAPI.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Comment> GetCommentByIdAsync(int commentId)
        {
            return await _context.Comments.Include(c => c.User).Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await _context.Comments.Include(c => c.User).Include(c => c.Post).ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(int postId)
        {
            return await _context.Comments.Include(c => c.User).Include(c => c.Post)
                .Where(c => c.PostId == postId).ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(int userId)
        {
            return await _context.Comments.Include(c => c.User).Include(c => c.Post)
                .Where(c => c.UserId == userId).ToListAsync();
        }

        /// <summary>
        /// Updates an existing comment in the database. Not persisted, use SaveChangesAsync to persist changes.
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<Comment> UpdateCommentAsync(Comment comment)
        {
            var commentToUpdate = await _context.Comments.FindAsync(comment.Id);
            if (commentToUpdate == null)
            {
                throw new KeyNotFoundException("Comment not found");
            }
            commentToUpdate.Content = comment.Content;
            commentToUpdate.CreatedAt = comment.CreatedAt;
            commentToUpdate.UserId = comment.UserId;
            commentToUpdate.PostId = comment.PostId;
            return commentToUpdate;
        }

        /// <summary>
        /// Deletes a comment from the database. Not persisted, use SaveChangesAsync to persist changes.
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return false;
            }
            _context.Comments.Remove(comment);
            return true;
        }

        /// <summary>
        /// Creates a new comment in the database. Not persisted, use SaveChangesAsync to persist changes.
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public async Task CreateCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a new comment in the database and returns its ID. 
        /// This method save changes immediately as to generate the ID.
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public async Task<int> CreateAndReturnIdAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment.Id;
        }
    }
}
