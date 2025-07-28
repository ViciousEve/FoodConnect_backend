using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Entities
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string IngredientsList { get; set; } // Comma-separated list of ingredients + ammount

        public string Description { get; set; } // A brief description of the post + steps

        public double? Calories { get; set; } // Optional, can be null

        public List<Media> Images { get; set; } // image(s) of the post

        public int Likes { get; set; }

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; } 
        public User User { get; set; } 

        public List<Comment> Comments { get; set; }

    }
}
