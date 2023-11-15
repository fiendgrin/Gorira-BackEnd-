﻿using Gorira.DataAccessLayer;
using Gorira.Models;
using Gorira.ViewModels;
using Gorira.ViewModels.ArtistVMs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using X.PagedList;

namespace Gorira.Controllers
{
    public class ArtistController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly int _pageSize;
        private readonly int _detailPageSize;
        public ArtistController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _pageSize = 12;
            _detailPageSize = 9;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(int? page, string? search, string? order = "popular")
        {
            if (page <= 0)
            {
                return NotFound();
            }

            IEnumerable<AppUser>? memberArtists = await _userManager
                .GetUsersInRoleAsync("Member");

            IEnumerable<AppUser>? Artists = new List<AppUser>();

            if (string.IsNullOrWhiteSpace(order) || (order != ("popular") && order != ("A-Z")))
            {
                return BadRequest();
            }

            Artists = order switch
            {
                "popular" => memberArtists
                                .Where(a => a.IsActive == true && a.NormalizedUserName != User.Identity?.Name?.Trim().ToUpperInvariant())
                                .OrderByDescending(a => a.Followers?.Count()),
                "A-Z" => memberArtists
                                .Where(a => a.IsActive == true && a.NormalizedUserName != User.Identity?.Name?.Trim().ToUpperInvariant())
                                .OrderBy(a => a.DisplayName)
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                Artists = Artists.Where(a => a.DisplayName.ToUpper().Contains(search.ToUpper()) ||
                 (a.Location != null && a.Location.ToUpper().Contains(search.ToUpper())));
            }



            return View(await Artists.ToPagedListAsync(page ?? 1, _pageSize));

        }

        public async Task<IActionResult> Detail(string? Id,int? page)
        {
            if (string.IsNullOrWhiteSpace(Id)) return BadRequest();


            AppUser? appUser = await _userManager.Users
                .Include(u=> u.Tracks.Where(t=>t.IsDeleted==false)).ThenInclude(t=>t.TrackTags.Where(tt=>tt.IsDeleted==false))
                .FirstOrDefaultAsync(u => u.Id == Id && u.IsActive == true);

            if (appUser == null) return NotFound();


            if (page <= 0) return NotFound();

            IPagedList<Track>? userTracks = null;

            if (appUser.Tracks != null)
            {
               userTracks = await appUser.Tracks.ToPagedListAsync(page ?? 1, _detailPageSize);
            }

            ArtistVM artistVM = new ArtistVM
            {
                User = appUser,
                Tracks = userTracks,
            };

            return View(artistVM);
        }
    }
}
