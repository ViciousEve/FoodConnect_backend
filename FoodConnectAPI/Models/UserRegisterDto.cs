using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Models
{
    public class UserRegisterDto
    {
        [Required]
        [MaxLength(255)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        [MinLength(8)]
        public string ConfirmPassword { get; set; }

        [Required]
        [MaxLength(30)]
        public string Region { get; set; }
    }
}
