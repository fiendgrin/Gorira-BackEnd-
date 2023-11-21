using Gorira.Models;
using X.PagedList;

namespace Gorira.ViewModels.ArtistVMs
{
    public class ArtistVM
    {
        public AppUser User { get; set; }

        public IPagedList<Track>? Tracks { get; set; }

        public bool? IsFollowed { get; set; }

        public AppUser? CurrentUser { get; set; }

    }
}
