using Gorira.Models;
using X.PagedList;

namespace Gorira.Areas.Manage.ViewModels.MoodVMs
{
    public class DetailMoodVM
    {
        public Mood Mood { get; set; }
        public IPagedList<Track>? PrimaryTracks { get; set; }
        public IPagedList<Track>? SecondaryTracks { get; set; }
    }
}
