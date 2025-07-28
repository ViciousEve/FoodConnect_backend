using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Entities
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // e.g., "Japanese", "Spicy", "Vegetarian"
        public List<PostTag> PostTags { get; set; }
    }
}
