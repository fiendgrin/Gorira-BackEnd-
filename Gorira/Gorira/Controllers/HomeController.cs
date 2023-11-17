using Gorira.DataAccessLayer;
using Gorira.Models;
using Gorira.ViewModels.HomeVMs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gorira.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //1.Index

        //==================================================

        //1.Index
        public async Task<IActionResult> Index()
        {
            DateTime date30DaysAgo = DateTime.Now.AddDays(-30);
            IEnumerable<Genre>? trendingGenres = null;

          

            if (await _context.Tracks.AnyAsync(t => t.IsDeleted == false && t.MainGenreId != null))
            {
                trendingGenres = await _context.Genres
                    .Where(g => g.IsDeleted == false)
                    .OrderByDescending(g => (g.SubGenreTracks.Any() ? g.SubGenreTracks.Count() : 0)+ g.MainGenreTracks.Count() )
                    .Take(4)
                    .ToListAsync();
            }

            HomeVM homeVMs = new HomeVM
            {
                Settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value),
                ReviewSliders = await _context.ReviewSliders.Where(rs => rs.IsDeleted == false).ToListAsync(),
                Sliders = await _context.Sliders.Where(s => s.IsDeleted == false).ToListAsync(),
                TrendingTracks = await _context.Tracks.Where(t => t.CreatedAt >= date30DaysAgo && t.IsDeleted == false).OrderByDescending(t => t.Plays).Take(4).ToListAsync(),
                TrendingGenres = trendingGenres,
                Users = await _userManager.Users.Where(u => u.IsActive == true).ToListAsync(),
            };

            return View(homeVMs);
        }
    }
}
