using FoodConnectAPI.Data;
using FoodConnectAPI.Interfaces;
using FoodConnectAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodConnectAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }
            _context.Users.Remove(user);
            return true;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.Include(u => u.Posts).Include(u => u.Comments).ToListAsync();
        }

        public async Task<User> GetByUserNameAsync(string userName)
        {
            return await _context.Users.Include(u => u.Posts).Include(u => u.Comments)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.Include(u => u.Posts).Include(u => u.Comments)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users.Include(u => u.Posts).Include(u => u.Comments)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            var userToUpdate = await _context.Users.FindAsync(user.Id);
            if (userToUpdate == null)
            {
                throw new KeyNotFoundException("User not found");
            }
            userToUpdate.UserName = user.UserName;
            userToUpdate.Email = user.Email;
            userToUpdate.PasswordHash = user.PasswordHash;
            userToUpdate.Role = user.Role;
            
            return userToUpdate;
        }
    }
}
