using Gorira.Models;
using X.PagedList;

namespace Gorira.ViewModels.TrackVMs
{
    public class TrackVM
    {
        public IPagedList<Track>? Tracks { get; set; }
        public List<AppUser>? Users { get; set; }
        public FilterVM filterVM { get; set; }
    }
}
