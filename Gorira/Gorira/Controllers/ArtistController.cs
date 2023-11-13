using Gorira.DataAccessLayer;
using Gorira.Models;
using Gorira.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Gorira.Controllers
{
    public class ArtistController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ArtistController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string? search , string? order = "popular")
        {
            IEnumerable<AppUser>? memberArtists = await _userManager
                .GetUsersInRoleAsync("Member");

            IEnumerable<AppUser>? Artists = new List<AppUser>();

            if (string.IsNullOrWhiteSpace(order) || ( order!=("popular") && order != ("A-Z")))
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

            if (!string.IsNullOrWhiteSpace(search) && search.Length>=2)
            {
                Artists = Artists.Where(a => a.DisplayName.ToUpper().Contains(search.ToUpper()) ||
                (a.Location !=null ? a.Location.ToUpper().Contains(search.ToUpper()) : false ) );
            }

            return View(Artists);

        }
    }
}
