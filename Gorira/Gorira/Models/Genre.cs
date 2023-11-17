using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gorira.Models
{
    public class Genre:BaseEntity
    {
        [StringLength(255)]
        public string Name { get; set; }
        [StringLength(255)]
        public string Image { get; set; }

        [InverseProperty("MainGenre")]
        public IEnumerable<Track>? MainGenreTracks { get; set; }
        [InverseProperty("SubGenre")]
        public IEnumerable<Track>? SubGenreTracks { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
