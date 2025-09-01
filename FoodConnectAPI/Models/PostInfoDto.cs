using FoodConnectAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Models
{
    public class PostInfoDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string IngredientsList { get; set; } // Comma-separated list of ingredients + ammount

        public string Description { get; set; } // A brief description of the post + steps

        public double? Calories { get; set; } // Optional, can be null

        public List<string> ImagesUrl { get; set; } // image(s) of the post

        public List<string> TagNames { get; set; } = new(); // List of tag names, e.g., "Japanese", "Spicy", "Vegetarian"
        
        public int Likes { get; set; }

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; } 

        public bool IsLikedByCurrentUser { get; set; }

    }
}
