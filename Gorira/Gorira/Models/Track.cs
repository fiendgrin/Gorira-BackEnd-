using Gorira.Attributes.ValidationAttributes;
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


        public IEnumerable<TrackTag>? TrackTags { get; set; }
        public IEnumerable<PlaylistTrack>? PlaylistTracks { get; set; }



        [NotMapped]
        [FileTypes("audio/mpeg")]
        [MaxFileSize(30)]
        public IFormFile? TaggedFile { get; set; }
        [NotMapped]
        [FileTypes("audio/mpeg", "audio/wav")]
        [MaxFileSize(150)]
        public IFormFile? UntaggedFile { get; set; }
        [NotMapped]
        [FileTypes("application/zip", "application/x-rar-compressed")]
        [MaxFileSize(300,ErrorMessage = "Track Stems File size must be under 300mb")]
        public IFormFile? TrackStemsFile { get; set; }
        [NotMapped]
        [FileTypes("image/png", "image/jpeg")]
        [MaxFileSize(10)]
        public IFormFile? CoverFile { get; set; }
    }
}
