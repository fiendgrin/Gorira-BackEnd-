using Gorira.DataAccessLayer;
using Gorira.Helpers;
using Gorira.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
        public async Task<IActionResult> Index()
        {

            return View();
        }
        [Authorize(Roles = "Member")]
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
