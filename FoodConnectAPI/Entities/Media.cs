using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Entities
{
    public class Media
    {
        public int Id { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}
