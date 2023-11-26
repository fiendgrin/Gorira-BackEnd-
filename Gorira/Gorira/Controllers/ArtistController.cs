using Gorira.DataAccessLayer;
using Gorira.Models;
using Gorira.ViewModels;
using Gorira.ViewModels.ArtistVMs;
using Microsoft.AspNetCore.Authorization;
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


        //1.Index
        //2.Detail
        //3.My Profile
        //4.Follow User
        //5.Report User
        //====================================================================

        //1.Index
        public async Task<IActionResult> Index(int? page, string? search, string? order = "popular")
        {
            if (page <= 0)
            {
                return NotFound();
            }

            IEnumerable<AppUser>? memberArtists = await _userManager.Users
                .Include(u => u.Followers.Where(f => f.IsDeleted == false))
                .Where(u => u.IsActive == true).ToListAsync();

            foreach (AppUser memberArtist in memberArtists)
            {
                memberArtist.Roles = await _userManager.GetRolesAsync(memberArtist);
            }

            if (string.IsNullOrWhiteSpace(order) || (order != ("popular") && order != ("A-Z")))
            {
                return BadRequest();
            }

            memberArtists = memberArtists.Where(a => a.Roles.Contains("Member"));

            memberArtists = order switch
            {
                "popular" => memberArtists
                                .Where(a => a.NormalizedUserName != User.Identity?.Name?.Trim().ToUpperInvariant())
                                .OrderByDescending(a => a.Followers == null ? 0 : a.Followers.Count()),
                "A-Z" => memberArtists
                                .Where(a => a.NormalizedUserName != User.Identity?.Name?.Trim().ToUpperInvariant())
                                .OrderBy(a => a.DisplayName),

                _ => memberArtists
                                .Where(a => a.NormalizedUserName != User.Identity?.Name?.Trim().ToUpperInvariant())
                                .OrderByDescending(a => a.Followers == null ? 0 : a.Followers.Count()),
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                memberArtists = memberArtists.Where(a => a.DisplayName.ToUpper().Contains(search.ToUpper()) ||
                 (a.Location != null && a.Location.ToUpper().Contains(search.ToUpper())));
            }



            return View(await memberArtists.ToPagedListAsync(page ?? 1, _pageSize));

        }

        //2.Detail
        public async Task<IActionResult> Detail(string? Id, int? page)
        {
            if (string.IsNullOrWhiteSpace(Id)) return BadRequest();

            AppUser? currentUser = null;
            if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
            {
                currentUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (currentUser.Id == Id)
                {
                    return RedirectToAction(nameof(MyProfile));
                }
            }
            AppUser? appUser = await _userManager.Users
                .Include(u => u.Followers.Where(f => f.IsDeleted == false))
                .Include(u => u.Tracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.TrackTags.Where(tt => tt.IsDeleted == false))
                .FirstOrDefaultAsync(u => u.Id == Id && u.IsActive == true);

            if (appUser == null) return NotFound();


            if (page <= 0) return NotFound();

            IPagedList<Track>? userTracks = null;

            if (appUser.Tracks != null)
            {
                userTracks = await appUser.Tracks.ToPagedListAsync(page ?? 1, _detailPageSize);
            }
            bool isFollower = false;
            if (currentUser != null && appUser.Followers.Any(u => u.FollowerId == currentUser.Id))
            {
                isFollower = true;
            }

            ArtistVM artistVM = new ArtistVM
            {
                User = appUser,
                Tracks = userTracks,
                IsFollowed = isFollower,
                CurrentUser = currentUser,
            };


            return View(artistVM);
        }

        //3.My Profile
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> MyProfile(int? page)
        {
            AppUser currentUser = await _userManager.Users
         .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);


            if (page <= 0) return NotFound();

            AppUser? appUser = await _userManager.Users
                 .Include(u => u.Followers)
              .Include(u => u.Tracks.Where(t => t.IsDeleted == false)).ThenInclude(t => t.TrackTags.Where(tt => tt.IsDeleted == false))
              .FirstOrDefaultAsync(u => u.Id == currentUser.Id && u.IsActive == true);

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

        //4.FollowUser
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> FollowUser(string? Id)
        {
            if (Id == null) return BadRequest();

            AppUser? appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == Id && u.IsActive == true);

            if (appUser == null) return NotFound();

            AppUser currentUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (Id == currentUser.Id) return NotFound();

            Follow? followCheck = await _context.Follows.FirstOrDefaultAsync(f => f.FolloweeId == Id && f.FollowerId == currentUser.Id);

            if (followCheck != null)
            {
                if (followCheck.IsDeleted == false)
                {
                    followCheck.IsDeleted = true;
                }
                else
                {
                    followCheck.IsDeleted = false;
                }

            }
            else
            {
                Follow follow = new Follow
                {
                    FolloweeId = Id,
                    FollowerId = currentUser.Id,
                    IsDeleted = false,
                    CreatedBy = currentUser.UserName,


                };
                await _context.Follows.AddAsync(follow);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        //5.Report User
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> ReportUser(string? Id)
        {
            if (Id == null) return BadRequest();

            AppUser? appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == Id && u.IsActive == true);

            if (appUser == null) return NotFound();

            AppUser currentUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (Id == currentUser.Id) return NotFound();

            if (!await _context.Reports.AnyAsync(r => r.SuspectId == Id && r.ReporterId == currentUser.Id))
            {
                Report report = new Report
                {
                    SuspectId = Id,
                    ReporterId = currentUser.Id,
                    IsDeleted = false,
                    CreatedBy = currentUser.UserName,
                };
                await _context.Reports.AddAsync(report);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Detail", new { Id = Id });
        }
    }
}
