using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int UserId { get; set; } // Foreign key to User
        public User User { get; set; } 

        public int PostId { get; set; } // Foreign key to Post
        public Post Post { get; set; }
    }
}
