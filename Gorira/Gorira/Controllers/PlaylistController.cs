using Gorira.DataAccessLayer;
using Gorira.Helpers;
using Gorira.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using X.PagedList;

namespace Gorira.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly int _pageSize;
        public PlaylistController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _pageSize = 12;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        //1.Index
        //2.Create

        //1.Index
        public async Task<IActionResult> Index(int? page, string? filter = "all")
        {
            if (page <= 0)
            {
                return NotFound();
            }

            IPagedList<Playlist> playlists = await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistFollowers.Where(pf => pf.IsDeleted == false))
                .Include(p => p.PlaylistTracks.Where(pt => pt.IsDeleted == false))
                .Where(p => p.IsDeleted == false)
                .OrderByDescending(p => p.CreatedAt).ToPagedListAsync(page ?? 1, _pageSize);

            AppUser? currentUser = await _userManager.Users
           .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser != null)
            {
                playlists = filter switch
                {
                    "all" => playlists,

                    "my" => playlists.Where(p => p.UserId == currentUser.Id).ToPagedList(page ?? 1, _pageSize),

                    "followed" => playlists.Where(p => p.PlaylistFollowers.Any(pf => pf.UserId == currentUser.Id)).ToPagedList(page ?? 1, _pageSize),
                    _ => playlists,

                };
            }

            return View(playlists);
        }
        [Authorize(Roles = "Member")]

        //2.Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Playlist? playlist)
        {

            if (playlist == null) return BadRequest();


            if (!ModelState.IsValid)
            {
                return View(playlist);
            }


            AppUser appUser = await _userManager.Users
           .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (playlist.CoverFile == null)
            {
                playlist.Cover = appUser.ProfilePicture;
            }
            else
            {

                playlist.Cover = await playlist.CoverFile.Save(_env.WebRootPath, new string[] { "assets", "images", "playlistCovers" });
            }

            playlist.CreatedBy = appUser.UserName;
            playlist.IsDeleted = false;
            playlist.UserId = appUser.Id;
            await _context.Playlists.AddAsync(playlist);
            await _context.SaveChangesAsync();



            return RedirectToAction(nameof(Index));
        }


    }
}
