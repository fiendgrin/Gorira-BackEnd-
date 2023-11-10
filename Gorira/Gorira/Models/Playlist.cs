using System.ComponentModel.DataAnnotations;

namespace Gorira.Models
{
    public class Playlist : BaseEntity
    {
        [StringLength(255)]
        public string Title { get; set; }
        [StringLength(255)]
        public string? Cover { get; set; }
        [StringLength(1000)]
        public string? Description { get; set; }

        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }
}
