using Gorira.Models;
using X.PagedList;

namespace Gorira.ViewModels.ArtistVMs
{
    public class ArtistVM
    {
        public AppUser User { get; set; }

        public IPagedList<Track>? Tracks { get; set; }
    }
}
