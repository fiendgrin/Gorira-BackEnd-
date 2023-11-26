using Gorira.Models;
using X.PagedList;

namespace Gorira.Areas.Manage.ViewModels.GenreVMs
{
    public class DetailGenreVM
    {
        public Genre Genre { get; set; }
        public IPagedList<Track>? MainTracks { get; set; }
        public IPagedList<Track>? SubTracks { get; set; }
    }
}
