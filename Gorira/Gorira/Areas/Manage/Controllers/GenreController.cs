using Gorira.Areas.Manage.ViewModels.GenreVMs;
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
    [Microsoft.AspNetCore.Authorization.Authorize(Roles ="SuperAdmin, Admin")]
    [Area("Manage")]
    public class GenreController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly int _pageSize;
        private readonly int _detailPageSize;
        public GenreController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
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

            IPagedList<Genre>? genres = await _context.Genres
              .Include(g=> g.MainGenreTracks.Where(t=>t.IsDeleted == false))
              .Include(g=> g.SubGenreTracks.Where(t => t.IsDeleted == false))
              .Where(g => g.IsDeleted == false).OrderByDescending(g=>g.Id).ToPagedListAsync(page ?? 1, _pageSize);

            return View(genres);
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

            Genre? genre = await _context.Genres
              .Include(g => g.SubGenreTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
              .Include(g => g.MainGenreTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
              .FirstOrDefaultAsync(g => g.IsDeleted == false && Id == g.Id);

            if (genre == null)
            {
                return NotFound();
            }

            IPagedList<Track>? mainTracks = await _context.Tracks.Where(g => g.IsDeleted == false && Id == g.MainGenreId).OrderByDescending(g => g.Id).ToPagedListAsync(page ?? 1, _detailPageSize);
            IPagedList<Track>? subTracks = await _context.Tracks.Where(g => g.IsDeleted == false && Id == g.SubGenreId).OrderByDescending(g => g.Id).ToPagedListAsync(subPage ?? 1, _detailPageSize);

            DetailGenreVM detailGenreVM = new DetailGenreVM 
            {
                Genre = genre,
                MainTracks = mainTracks,
                SubTracks = subTracks,
            };

            return View(detailGenreVM);
        }

        public async Task<IActionResult> Create() 
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Genre genre)
        {

            if (string.IsNullOrWhiteSpace(genre.Name))
            {
                ModelState.AddModelError("Name", "Name field is required");
                return View(genre);
            }


            if (await _context.Genres.AnyAsync(g=>g.IsDeleted == false && g.Name.Trim().ToLower() == genre.Name.Trim().ToLower()))
            {
                ModelState.AddModelError("Name", "Name should be unique");
                return View(genre);
            }



            if (genre.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Image is required");
                return View(genre);
               
            }

            if (!ModelState.IsValid) return View(genre);

            genre.Image = await genre.ImageFile.Save(_env.WebRootPath, new string[] { "assets", "images", "genres" });
            genre.CreatedBy = User.Identity.Name;

            await _context.Genres.AddAsync(genre);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Genre");
        }

        public async Task<IActionResult> Update(int? Id)
        {
            if (Id == null) return BadRequest();

            Genre? genre = await _context.Genres.FirstOrDefaultAsync(g => g.Id == Id && g.IsDeleted == false);

            if (genre == null) return NotFound();

            return View(genre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? Id,Genre genre)
        {
            if (Id == null) return BadRequest();

            if (genre == null) return BadRequest();

            Genre? DbGenre = await _context.Genres.FirstOrDefaultAsync(g => g.Id == Id && g.IsDeleted == false);

            if (DbGenre == null) return BadRequest();

            if (await _context.Genres.AnyAsync(g => g.IsDeleted == false && (g.Name.Trim().ToLower() == genre.Name.Trim().ToLower() && g.Id != Id) ) )
            {
                ModelState.AddModelError("Name", "Name should be unique");
                return View(genre);
            }

            if (!ModelState.IsValid) return View(genre);

            if (genre.ImageFile != null)
            {
                    string filePath = Path.Combine(_env.WebRootPath, "assets", "images", "genres", DbGenre.Image);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                

                DbGenre.Image = await genre.ImageFile.Save(_env.WebRootPath, new string[] { "assets", "images", "genres" });
            }

            DbGenre.Name = genre.Name;
            DbGenre.UpdatedBy = User.Identity.Name;
            DbGenre.UpdatedAt = DateTime.Now;


            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Genre");
        }

        public async Task<IActionResult> Delete(int? Id) 
        {
            if (Id == null) return BadRequest();

            ViewBag.Genres = await _context.Genres
                 .Include(g => g.SubGenreTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
              .Include(g => g.MainGenreTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
                .Where(g => g.IsDeleted == false && g.Id != Id).ToListAsync();

            Genre? genre = await _context.Genres
              .Include(g => g.SubGenreTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
              .Include(g => g.MainGenreTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
              .FirstOrDefaultAsync(g => g.IsDeleted == false && Id == g.Id);

            if (genre == null) return NotFound();

            return View(genre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGenre(int? Id, int? newGenreId) 
        {
            if (Id == null) return BadRequest();

            if (newGenreId == null) return BadRequest();

            Genre? genre = await _context.Genres.FirstOrDefaultAsync(g => g.IsDeleted == false && Id == g.Id);

            if (genre == null) return NotFound();

            ViewBag.Genres = await _context.Genres
                .Include(g => g.SubGenreTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
             .Include(g => g.MainGenreTracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.User)
               .Where(g => g.IsDeleted == false && g.Id != Id).ToListAsync();

            if (newGenreId == genre.Id) return BadRequest();

            if (!await _context.Genres.AnyAsync(g => g.IsDeleted == false && newGenreId == g.Id)) return NotFound();

           IEnumerable<Track>? tracksMain = await _context.Tracks.Where(t => t.IsDeleted == false && Id == t.MainGenreId).ToListAsync();
            foreach (Track track in tracksMain)
            {
                track.MainGenreId = newGenreId;
                track.UpdatedAt = DateTime.UtcNow;
                track.UpdatedBy = "System";
            }
           IEnumerable<Track> tracksSub = await _context.Tracks.Where(t => t.IsDeleted == false && Id == t.SubGenreId).ToListAsync();
            foreach (Track track in tracksSub)
            {
                track.SubGenreId = newGenreId;
                track.UpdatedAt = DateTime.UtcNow;
                track.UpdatedBy = "System";
            }
            genre.DeletedBy = User.Identity.Name;
            genre.DeletedAt = DateTime.Now;
            genre.IsDeleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Genre");
        }
    }
}
