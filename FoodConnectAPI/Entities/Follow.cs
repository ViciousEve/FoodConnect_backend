﻿using System.ComponentModel.DataAnnotations;

namespace FoodConnectAPI.Entities
{
    public class Follow
    {
        public int Id { get; set; }

        public int FollowerId { get; set; }
        public User Follower { get; set; }

        public int FollowedId { get; set; }
        public User Followed { get; set; }

        [Required]
        public DateTime FollowedAt { get; set; } = DateTime.Now;
    }
}
