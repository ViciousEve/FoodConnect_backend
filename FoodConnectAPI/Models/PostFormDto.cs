using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FoodConnectAPI.Models
{
    public class PostFormDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
        [Required]
        [MaxLength(1000)]
        public string IngredientsList { get; set; }
        [Required]
        [MaxLength(2000)]
        public string Description { get; set; }
        public double? Calories { get; set; }
        public List<string> TagNames { get; set; } = new List<string>();
        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<IFormFile> ImageFiles { get; set; } = new List<IFormFile>();
    }
}
