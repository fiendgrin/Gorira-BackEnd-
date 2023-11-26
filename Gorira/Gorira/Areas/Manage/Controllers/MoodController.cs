using Gorira.Areas.Manage.ViewModels.MoodVMs;
using Gorira.DataAccessLayer;
using Gorira.Helpers;
using Gorira.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Gorira.Areas.Manage.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "SuperAdmin, Admin")]
    [Area("Manage")]
    public class MoodController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly int _pageSize;
        private readonly int _detailPageSize;
        public MoodController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _pageSize = 10;
            _detailPageSize = 5;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }


        public async Task<IActionResult> Index(int? page)
        {
            if (page <= 0)
            {
                return NotFound();
            }
            ViewBag.Counter = page == null ? 0 : (page - 1) * _pageSize;

            IPagedList<Mood>? moods = await _context.Moods
              .Include(m => m.PrimaryMoodTracks.Where(t => t.IsDeleted == false))
              .Include(m => m.SecondaryMoodTracks.Where(t => t.IsDeleted == false))
              .Where(m => m.IsDeleted == false).OrderByDescending(m => m.Id).ToPagedListAsync(page ?? 1, _pageSize);

            return View(moods);
        }

        public async Task<IActionResult> Detail(int? Id, int? page, int? subPage)
        {
            if (page <= 0)
            {
                return NotFound();
            }
            if (subPage <= 0)
            {
                return NotFound();
            }


            if (Id == null)
            {
                return BadRequest();
            }

            Mood? mood = await _context.Moods
              .Include(m => m.PrimaryMoodTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
              .Include(m => m.SecondaryMoodTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
              .FirstOrDefaultAsync(m => m.IsDeleted == false && Id == m.Id);

            if (mood == null)
            {
                return NotFound();
            }

            IPagedList<Track>? primaryTracks = await _context.Tracks.Where(g => g.IsDeleted == false && Id == g.PrimaryMoodId).OrderByDescending(g => g.Id).ToPagedListAsync(page ?? 1, _detailPageSize);
            IPagedList<Track>? secondaryTracks = await _context.Tracks.Where(g => g.IsDeleted == false && Id == g.SecondaryMoodId).OrderByDescending(g => g.Id).ToPagedListAsync(subPage ?? 1, _detailPageSize);

            DetailMoodVM detailMoodVM = new DetailMoodVM
            {
                Mood = mood,
                PrimaryTracks = primaryTracks,
                SecondaryTracks = secondaryTracks
            };

            return View(detailMoodVM);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mood mood)
        {

            if (string.IsNullOrWhiteSpace(mood.Name))
            {
                ModelState.AddModelError("Name", "Name field is required");
                return View(mood);
            }


            if (await _context.Moods.AnyAsync(g => g.IsDeleted == false && g.Name.Trim().ToLower() == mood.Name.Trim().ToLower()))
            {
                ModelState.AddModelError("Name", "Name should be unique");
                return View(mood);
            }

            if (!ModelState.IsValid) return View(mood);

            mood.CreatedBy = User.Identity.Name;

            await _context.Moods.AddAsync(mood);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Mood");
        }

        public async Task<IActionResult> Update(int? Id)
        {
            if (Id == null) return BadRequest();

            Mood? mood = await _context.Moods.FirstOrDefaultAsync(g => g.Id == Id && g.IsDeleted == false);

            if (mood == null) return NotFound();

            return View(mood);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? Id, Mood mood)
        {
            if (Id == null) return BadRequest();

            if (mood == null) return BadRequest();

            Mood? DbMood = await _context.Moods.FirstOrDefaultAsync(g => g.Id == Id && g.IsDeleted == false);

            if (DbMood == null) return BadRequest();

            if (await _context.Moods.AnyAsync(g => g.IsDeleted == false && (g.Name.Trim().ToLower() == mood.Name.Trim().ToLower() && g.Id != Id)))
            {
                ModelState.AddModelError("Name", "Name should be unique");
                return View(mood);
            }

            if (!ModelState.IsValid) return View(mood);


            DbMood.Name = mood.Name;
            DbMood.UpdatedBy = User.Identity.Name;
            DbMood.UpdatedAt = DateTime.Now;


            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Mood");
        }

        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return BadRequest();

            ViewBag.Moods = await _context.Moods
                 .Include(g => g.PrimaryMoodTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
              .Include(g => g.SecondaryMoodTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
                .Where(g => g.IsDeleted == false && g.Id != Id).ToListAsync();

            Mood? mood = await _context.Moods
              .Include(g => g.PrimaryMoodTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
              .Include(g => g.SecondaryMoodTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
              .FirstOrDefaultAsync(g => g.IsDeleted == false && Id == g.Id);

            if (mood == null) return NotFound();

            return View(mood);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMood(int? Id, int? newMoodId)
        {
            if (Id == null) return BadRequest();

            if (newMoodId == null) return BadRequest();

            Mood? mood = await _context.Moods.FirstOrDefaultAsync(g => g.IsDeleted == false && Id == g.Id);

            if (mood == null) return NotFound();

            ViewBag.Moods = await _context.Moods
                 .Include(g => g.PrimaryMoodTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
              .Include(g => g.SecondaryMoodTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
                .Where(g => g.IsDeleted == false && g.Id != Id).ToListAsync();

            if (newMoodId == mood.Id) return BadRequest();

            if (!await _context.Moods.AnyAsync(g => g.IsDeleted == false && newMoodId == g.Id)) return NotFound();

            IEnumerable<Track>? tracksPrimary = await _context.Tracks.Where(t => t.IsDeleted == false && Id == t.PrimaryMoodId).ToListAsync();
            foreach (Track track in tracksPrimary)
            {
                track.PrimaryMoodId = newMoodId;
                track.UpdatedAt = DateTime.UtcNow;
                track.UpdatedBy = "System";
            }

            IEnumerable<Track> tracksSecondary = await _context.Tracks.Where(t => t.IsDeleted == false && Id == t.SecondaryMoodId).ToListAsync();
            foreach (Track track in tracksSecondary)
            {
                track.SecondaryMoodId = newMoodId;
                track.UpdatedAt = DateTime.UtcNow;
                track.UpdatedBy = "System";
            }

            mood.DeletedBy = User.Identity.Name;
            mood.DeletedAt = DateTime.Now;
            mood.IsDeleted = true;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Mood");
        }
    }
}
