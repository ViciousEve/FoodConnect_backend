namespace FoodConnectAPI.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Region { get; set; }
        public string? ProfilePictureUrl { get; set; } // optional profile picture
        public string Token { get; set; }
    }
}
