using Gorira.DataAccessLayer;
using Gorira.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Gorira.Controllers
{
    public class FeedController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly int _pageSize;
        public FeedController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _pageSize = 12;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult>  Index(int? page)
        {
            if (page <= 0) return NotFound();
            AppUser? appUser = await _userManager.Users.FirstOrDefaultAsync(u=>u.UserName == User.Identity.Name);
            IPagedList<Track>? tracks = null;
            if (appUser != null) 
            {
               tracks = await _context.Tracks
                    .Include(t=>t.User)
                    .Where(t => t.IsDeleted == false && (t.User.Followers !=null &&  t.User.Followers.Any(f => f.FollowerId == appUser.Id && f.IsDeleted == false)))
                    .OrderByDescending(f => f.CreatedAt)
                    .ToPagedListAsync(page ?? 1,_pageSize);

            } 


            return View(tracks);
        }
    }
}
