using Microsoft.AspNetCore.Http;

namespace FoodConnectAPI.Models
{
    public class PostFormDto
    {
        public string Title { get; set; }
        public string IngredientsList { get; set; }
        public string Description { get; set; }
        public double? Calories { get; set; }
        public List<string> TagNames { get; set; } = new List<string>();
        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<IFormFile> ImageFiles { get; set; } = new List<IFormFile>();
    }
}
