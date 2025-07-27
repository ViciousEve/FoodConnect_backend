using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Models
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

        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(10)]
        public string Role { get; set; } //user, admin

        [Required]
        [MaxLength(30)]
        public string Region { get; set; } //e.g., "America", "Europe", "Asia" , etc.
        public int TotalLikesReceived { get; set; } // Total likes received by the user

        public List<Post> Posts { get; set; } //Users posts
        public List<Comment> Comments { get; set; }

    }
}
