namespace Gorira.Models
{
    public class PlaylistTrack:BaseEntity
    {
        public int? TrackId { get; set; }
        public Track? Track { get; set; }
        public int? PlaylistId { get; set; }
        public Playlist? Playlist { get; set; }
    }
}
