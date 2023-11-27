using Gorira.Models;
using X.PagedList;

namespace Gorira.Areas.Manage.ViewModels.UserVMs
{
    public class UserDetailVM
    {
        public AppUser AppUser { get; set; }
        public IPagedList<Track>? Tracks { get; set; }
    }
}
