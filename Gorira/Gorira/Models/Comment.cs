using System.ComponentModel.DataAnnotations;

namespace Gorira.Models
{
    public class Comment:BaseEntity
    {
        [StringLength(500)]
        public string Text { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public int? TrackId { get; set; }
        public  Track? Track { get; set; }
    }
}
