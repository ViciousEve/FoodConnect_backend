namespace FoodConnectAPI.Models
{
    public class PostFormDto
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string IngredientsList { get; set; }
        public string Description { get; set; }
        public double? Calories { get; set; }
        public List<string> TagNames { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<IFormFile> ImageFiles { get; set; }
    }
}
