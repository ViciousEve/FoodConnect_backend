namespace FoodConnectAPI.Models
{
    public class ReportInfoDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int PostId { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
