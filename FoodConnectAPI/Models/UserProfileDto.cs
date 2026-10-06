namespace FoodConnectAPI.Models
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Region { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int TotalLikesReceived { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int PostCount { get; set; }
    }
}
