using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Models
{
    public class CreateReportDto
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }
    }
}
