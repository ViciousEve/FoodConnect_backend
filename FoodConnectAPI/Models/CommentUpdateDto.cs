using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Models
{
    public class CommentUpdateDto
    {
        [Required]
        [MaxLength(500)]
        public string Content { get; set; } // The updated content of the comment
    }
}
