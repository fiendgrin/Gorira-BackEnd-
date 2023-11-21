using Gorira.Attributes.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public IEnumerable<PlaylistTrack>? PlaylistTracks { get; set; }
        public IEnumerable<PlaylistFollower>? PlaylistFollowers { get; set; }

        [NotMapped]
        [FileTypes("image/png", "image/jpeg")]
        [MaxFileSize(10)]
        public IFormFile? CoverFile { get; set; }
    }
}
