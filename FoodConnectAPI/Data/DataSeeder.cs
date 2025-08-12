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

            // Check if database has any posts
            if (!await context.Posts.AnyAsync())
            {
                await SeedTestPostsAsync(context);
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

            var user1 = new User
            {
                UserName = "foodie1",
                Email = "foodie1@foodconnect.com",
                PasswordHash = HashHelper.HashPassword("password1"),
                Role = "user",
                Region = "Asia",
                TotalLikesReceived = 0
            };

            var user2 = new User
            {
                UserName = "chefemma",
                Email = "emma@foodconnect.com",
                PasswordHash = HashHelper.HashPassword("password2"),
                Role = "user",
                Region = "Europe",
                TotalLikesReceived = 0
            };

            // Add users to context
            await context.Users.AddRangeAsync(adminUser, user1, user2);
        }

        private static async Task SeedTestPostsAsync(AppDbContext context)
        {
            // Get users
            var users = await context.Users.ToListAsync();
            var admin = users.FirstOrDefault(u => u.UserName == "admin");
            var user1 = users.FirstOrDefault(u => u.UserName == "foodie1");
            var user2 = users.FirstOrDefault(u => u.UserName == "chefemma");

            // Tags
            var tags = new[]
            {
                "latin american", "street food", "handheld",
                "south east asian", "noodle soup", "brothbased",
                "european", "raw dish", "fine dining", "appetizer"
            };
            var tagEntities = new List<Tag>();
            foreach (var tag in tags.Select(t => t.ToLowerInvariant()).Distinct())
            {
                var tagEntity = new Tag { Name = tag };
                tagEntities.Add(tagEntity);
            }
            await context.Tags.AddRangeAsync(tagEntities);
            await context.SaveChangesAsync();

            // Helper to get tag by name
            Tag GetTag(string name) => tagEntities.First(t => t.Name == name.ToLowerInvariant());

            // Posts
            var now = DateTime.UtcNow;
            var posts = new List<Post>();

            var post1 = new Post
            {
                Title = "Tacos",
                IngredientsList = "Tortilla, Beef, Onion, Cilantro, Salsa",
                Description = "Classic Latin American street food tacos with beef, onion, cilantro, and salsa.",
                CreatedAt = now.AddDays(-7),
                User = admin,
                Images = new List<Media> { new Media { Url = "https://tse4.mm.bing.net/th/id/OIP.Kdcfg93JXy4O0lFC4s-AqwHaEh?rs=1&pid=ImgDetMain&o=7&rm=3" } },
                PostTags = new List<PostTag>
                {
                    new PostTag { Tag = GetTag("latin american") },
                    new PostTag { Tag = GetTag("street food") },
                    new PostTag { Tag = GetTag("handheld") }
                },
                Comments = new List<Comment>
                {
                    new Comment { User = user1, Content = "Looks delicious!", CreatedAt = now.AddDays(-7).AddHours(2) },
                    new Comment { User = user2, Content = "I want to try this recipe!", CreatedAt = now.AddDays(-7).AddHours(3) }
                }
            };

            var post2 = new Post
            {
                Title = "Pho",
                IngredientsList = "Rice noodles, Beef, Broth, Herbs, Onion",
                Description = "Traditional Vietnamese noodle soup with rich broth and fresh herbs.",
                CreatedAt = now.AddDays(-5),
                User = user1,
                Images = new List<Media> { new Media { Url = "https://tse3.mm.bing.net/th/id/OIP.l9zl1iWD7oVf9NDi9hRV_wHaEK?rs=1&pid=ImgDetMain&o=7&rm=3" } },
                PostTags = new List<PostTag>
                {
                    new PostTag { Tag = GetTag("south east asian") },
                    new PostTag { Tag = GetTag("noodle soup") },
                    new PostTag { Tag = GetTag("brothbased") }
                },
                Comments = new List<Comment>
                {
                    new Comment { User = admin, Content = "Pho is my comfort food!", CreatedAt = now.AddDays(-5).AddHours(1) },
                    new Comment { User = user2, Content = "Great photo and recipe!", CreatedAt = now.AddDays(-5).AddHours(2) },
                    new Comment { User = user1, Content = "Thank you!", CreatedAt = now.AddDays(-5).AddHours(3) }
                }
            };

            var post3 = new Post
            {
                Title = "Beef Tatare",
                IngredientsList = "Beef, Egg yolk, Capers, Onion, Spices",
                Description = "European fine dining raw beef dish, served as an appetizer.",
                CreatedAt = now.AddDays(-2),
                User = user2,
                Images = new List<Media> { new Media { Url = "https://tse4.mm.bing.net/th/id/OIP.gq8aSyPpca5pJkN4qDsbmgHaGl?rs=1&pid=ImgDetMain&o=7&rm=3" } },
                PostTags = new List<PostTag>
                {
                    new PostTag { Tag = GetTag("european") },
                    new PostTag { Tag = GetTag("raw dish") },
                    new PostTag { Tag = GetTag("fine dining") },
                    new PostTag { Tag = GetTag("appetizer") }
                },
                Comments = new List<Comment>
                {
                    new Comment { User = admin, Content = "So elegant!", CreatedAt = now.AddDays(-2).AddHours(1) },
                    new Comment { User = user1, Content = "Never tried raw beef before!", CreatedAt = now.AddDays(-2).AddHours(2) }
                }
            };

            posts.Add(post1);
            posts.Add(post2);
            posts.Add(post3);

            await context.Posts.AddRangeAsync(posts);
        }
    }
} 