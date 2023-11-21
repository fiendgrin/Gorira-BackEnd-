using Gorira.Models;

namespace Gorira.ViewModels.PlalistVMs
{
    public class PlaylistDetailVM
    {
        public Playlist playlist { get; set; }
        public AppUser? User { get; set; }
    }
}
