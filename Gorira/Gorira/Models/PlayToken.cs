namespace Gorira.Models
{
    public class PlayToken : BaseEntity
    {
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public int? TrackId { get; set; }
        public Track? Track { get; set; }
    }
}
