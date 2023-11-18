using Gorira.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gorira.DataAccessLayer
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext>options):base(options) {}

        public DbSet<Setting> Settings { get; set; }
        public DbSet<Slider> Sliders { get; set; }
        public DbSet<ReviewSlider> ReviewSliders { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TrackTag> TrackTags { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Mood> Moods { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistTrack> PlaylistTracks { get; set; }
        public DbSet<PlaylistFollower> PlaylistFollowers { get; set; }
        public DbSet<PlayToken> PlayTokens { get; set; }
        public DbSet<Basket> Baskets { get; set; }
    }
}
