namespace FoodConnectAPI.Models
{
    public class CommentInfoDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // User info
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
