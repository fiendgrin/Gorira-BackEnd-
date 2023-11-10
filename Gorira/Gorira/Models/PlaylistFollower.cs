namespace Gorira.Models
{
    public class PlaylistFollower : BaseEntity
    {
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public int? PlaylistId { get; set; }
        public Playlist? Playlist { get; set; }
    }
}
