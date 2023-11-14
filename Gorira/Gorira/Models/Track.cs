using Gorira.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gorira.Models
{
    public class Track : BaseEntity
    {
        [StringLength(255)]
        public string Title { get; set; }
        [StringLength(255)]
        public string? Tagged { get; set; }
        [StringLength(255)]
        public string? Untagged { get; set; }
        [StringLength(255)]
        public string? TrackStems { get; set; }
        [StringLength(255)]
        public string? Cover { get; set; }
        [Range(0,9999)]
        public double Price { get; set; }
        [Range(0, 9999)]
        public double? UnlimitedPrice { get; set; }
        [StringLength(1000)]
        public string? Description { get; set; }
        [Range(0, 9999)]
        public double? Bpm { get; set; }
        [Range(0,int.MaxValue)]
        public int? Plays { get; set; }
        public Key MusicKey { get; set; }
        public bool HasFree { get; set; }

        public AppUser? User { get; set; }
        public string? UserId { get; set; }
        [ForeignKey("MainGenreId")]
        public Genre? MainGenre { get; set; }
        public int? MainGenreId { get; set; }
        [ForeignKey("SubGenreId")]
        public Genre? SubGenre { get; set; }
        public int? SubGenreId { get; set; }
        [ForeignKey("PrimaryMoodId")]
        public Mood? PrimaryMood { get; set; }
        public int? PrimaryMoodId { get; set; }
        [ForeignKey("SecondaryMoodId")]
        public Mood? SecondaryMood { get; set; }
        public int? SecondaryMoodId { get; set; }


        [NotMapped]
        public IFormFile? TaggedFile { get; set; }
        [NotMapped]
        public IFormFile? UntaggedFile { get; set; }
        [NotMapped]
        public IFormFile ? TrackStemsFile { get; set; }
        [NotMapped]
        public IFormFile? CoverFile { get; set; }
    }
}
