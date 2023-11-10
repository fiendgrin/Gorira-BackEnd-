using System.ComponentModel.DataAnnotations.Schema;

namespace Gorira.Models
{
    public class Follow:BaseEntity
    {
        // Follower (the one who follows)
        public string? FollowerId { get; set; }
        [ForeignKey("FollowerId")]
        public AppUser? Follower { get; set; }

        // Followee (the one who gets followed)
        public string? FolloweeId { get; set; }
        [ForeignKey("FolloweeId")]
        public AppUser? Followee { get; set; }
    }
}
