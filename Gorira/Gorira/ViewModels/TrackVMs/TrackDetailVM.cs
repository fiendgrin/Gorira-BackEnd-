using Gorira.Models;

namespace Gorira.ViewModels.TrackVMs
{
    public class TrackDetailVM
    {
        public Track? track { get; set; }

        public IEnumerable<Playlist>? playlists { get; set; }
    }
}
