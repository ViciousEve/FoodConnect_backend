using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Models
{
    public class CommentAddDto
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(500)]
        public string Content { get; set; } = string.Empty;

    }
}
