using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(10)]
        public string Role { get; set; } //user, admin

        [Required]
        [MaxLength(30)]
        public string Region { get; set; } //e.g., "America", "Europe", "Asia" , etc.
        public int TotalLikesReceived { get; set; } // Total likes received by the user

        public string? ProfilePictureUrl { get; set; } //optional profile picture

        public List<Post> Posts { get; set; } //Users posts
        public List<Comment> Comments { get; set; }
        public List<Like> Likes { get; set; }
        public List<Follow> Followers { get; set; } // Users who follow this user
        public List<Follow> Following { get; set; } // Users this user follows
        public List<Report> Reports { get; set; }
    }
}
