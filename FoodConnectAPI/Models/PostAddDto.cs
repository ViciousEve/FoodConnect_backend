using FoodConnectAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Models
{
    public class PostAddDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string IngredientsList { get; set; } // Comma-separated list of ingredients + ammount

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } // A brief description of the post + steps

        public double? Calories { get; set; } // Optional, can be null

        public List<string> ImageUrls { get; set; } = new(); // image(s) of the post

        //public List<IFormFile> ImageFiles { get; set; } = new(); // if uploading
        public List<string> TagNames { get; set; } = new(); // List of tag names, e.g., "Japanese", "Spicy", "Vegetarian"
      
    }
}
