using FoodConnectAPI.Entities;
using FoodConnectAPI.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FoodConnectAPI.Data
{
    public static class DataSeeder
    {
        public static async Task SeedTestDataAsync(AppDbContext context)
        {
            // Check if database has any users
            if (!await context.Users.AnyAsync())
            {
                await SeedTestUsersAsync(context);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedTestUsersAsync(AppDbContext context)
        {
            // Create test admin user
            var adminUser = new User
            {
                UserName = "admin",
                Email = "admin@foodconnect.com",
                PasswordHash = HashHelper.HashPassword("admin12"),
                Role = "admin",
                Region = "America",
                TotalLikesReceived = 0
            };

            // Add admin user to context
            await context.Users.AddAsync(adminUser);
        }
    }
} 