using Gorira.DataAccessLayer;
using Gorira.Helpers;
using Gorira.Models;
using Gorira.ViewModels.PlalistVMs;
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
        //3.AddTrack
        //4.Detail
        //5.Edit
        //6.Delete
        //7.Follow Playlist

        //==========================================

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

        //3.AddTrack
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> AddTrack(int? trackId,int? playlistId,bool? reload=false) 
        {
            if (trackId == null) return BadRequest();

            if (playlistId == null) return BadRequest();

            Track? track = await _context.Tracks.FirstOrDefaultAsync(t => t.IsDeleted == false && trackId == t.Id);

            if (track == null) return NotFound();

            Playlist? playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.IsDeleted == false && playlistId == p.Id);

            if (playlist == null) return NotFound();

            AppUser appUser = await _userManager.Users
           .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name); ;

            IEnumerable<PlaylistTrack> playlistTracks = await _context.PlaylistTracks.Where(pt => pt.IsDeleted == false).ToListAsync();
            PlaylistTrack? playlistTrack = playlistTracks.FirstOrDefault(pt => pt.TrackId == trackId && playlistId == pt.PlaylistId);
            if (playlistTrack !=null)
            {
                if (playlistTrack.IsDeleted == false)
                {
                    playlistTrack.IsDeleted = true;
                }
                else 
                {
                    playlistTrack.IsDeleted = false;
                }

            }
            else
            {
                playlistTrack = new PlaylistTrack
                {
                    TrackId = trackId,
                    PlaylistId = playlistId,
                    CreatedBy = appUser.UserName,
                    IsDeleted = false,
                };

                await _context.PlaylistTracks.AddAsync(playlistTrack);
            }

           
            await _context.SaveChangesAsync();

            if (reload == true)
            {
               return RedirectToAction("Edit", new {Id= playlistId });
            }

            return Ok();
        }

        //4.Detail
        public async Task<IActionResult> Detail(int? Id) 
        {
            if(Id == null) return BadRequest();

            Playlist? playlist = await _context.Playlists
                .Include(p=>p.User)
                .Include(p=>p.PlaylistFollowers.Where(pf=>pf.IsDeleted == false))
                .Include(p=>p.PlaylistTracks.Where(pt=>pt.IsDeleted==false)).ThenInclude(pt=>pt.Track).ThenInclude(t=>t.User)
                .FirstOrDefaultAsync(p => p.Id == Id && p.IsDeleted == false);

            if (playlist == null) return NotFound();

            AppUser? appUser = await _userManager.Users
           .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            PlaylistDetailVM playlistDetailVM = new PlaylistDetailVM
            {
                playlist= playlist,
                User=appUser,

            };
         

            return View(playlistDetailVM);
        }

        //5.Edit
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null) return BadRequest();

            AppUser appUser = await _userManager.Users
     .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            Playlist? playlist = await _context.Playlists
                .Include(p=>p.User)
                 .Include(p => p.PlaylistTracks.Where(pt => pt.IsDeleted == false)).ThenInclude(pt => pt.Track).ThenInclude(t => t.User)
                .FirstOrDefaultAsync(p => p.Id == Id && p.IsDeleted == false && p.UserId == appUser.Id);

            if (playlist == null) return NotFound();

            return View(playlist);
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Playlist? playlist, int? Id) 
        {
            if (playlist == null) return BadRequest();


            if (Id == null) return BadRequest();

            Playlist DbPlaylist = await _context.Playlists.FirstOrDefaultAsync(p => p.Id == playlist.Id);

            if (!ModelState.IsValid)
            {
                return View(playlist);
            }
            AppUser appUser = await _userManager.Users
         .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (playlist.CoverFile != null)
            {
                if (playlist.Cover != appUser.ProfilePicture && DbPlaylist.Cover != null)
                {
                    string filePath = Path.Combine(_env.WebRootPath, "assets", "images", "playlistCovers", DbPlaylist.Cover);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                  
                }

                DbPlaylist.Cover = await playlist.CoverFile.Save(_env.WebRootPath, new string[] { "assets", "images", "playlistCovers" });
            }

            DbPlaylist.Title = playlist.Title;
            DbPlaylist.Description = playlist.Description;

            DbPlaylist.UpdatedBy = appUser.UserName;
            DbPlaylist.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //6.Delete
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Delete(int? Id) 
        {
            if (Id == null) return BadRequest();

            Playlist? playlist = await _context.Playlists
                .Include(p=>p.PlaylistTracks.Where(pt=>pt.IsDeleted == false))
                .Include(p => p.PlaylistFollowers.Where(pf => pf.IsDeleted == false))
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.IsDeleted == false && t.Id == Id);

            if (playlist == null) return NotFound();

            AppUser appUser = await _userManager.Users
           .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (playlist.UserId != appUser.Id) return NotFound();

            foreach (PlaylistTrack playlistTrack in playlist.PlaylistTracks)
            {
                playlistTrack.IsDeleted = true;
                playlistTrack.DeletedBy = appUser.UserName;
                playlistTrack.DeletedAt = DateTime.Now;
            }

            foreach (PlaylistFollower playlistFollower in playlist.PlaylistFollowers)
            {
                playlistFollower.IsDeleted = true;
                playlistFollower.DeletedBy = appUser.UserName;
                playlistFollower.DeletedAt = DateTime.Now;
            }


            playlist.IsDeleted = true;
            playlist.DeletedBy = appUser.UserName;
            playlist.DeletedAt = DateTime.Now;


            await _context.SaveChangesAsync();


            if (playlist.Cover != appUser.ProfilePicture && playlist.Cover!= null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "images", "playlistCovers", playlist.Cover);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }


            return RedirectToAction(nameof(Index));
        }

        //7.Follow Playlist
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> FollowPlaylist(int? Id) 
        {

            if (Id == null) return BadRequest();

            Playlist? playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.Id == Id && p.IsDeleted == false);

            if (playlist == null) return NotFound();

            AppUser currentUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser.Id == playlist.UserId) return NotFound();

            PlaylistFollower? playlistFollowerCheck = await _context.PlaylistFollowers.FirstOrDefaultAsync(pf=>pf.PlaylistId == Id && currentUser.Id == pf.UserId);

            if (playlistFollowerCheck != null)
            {
                if (playlistFollowerCheck.IsDeleted == false)
                {
                    playlistFollowerCheck.IsDeleted = true;
                }
                else
                {
                    playlistFollowerCheck.IsDeleted = false;
                }

            }
            else
            {
                PlaylistFollower playlistFollower = new PlaylistFollower
                {
                    UserId = currentUser.Id,
                    PlaylistId = Id,
                    IsDeleted = false,
                    CreatedBy = currentUser.UserName,


                };
                await _context.PlaylistFollowers.AddAsync(playlistFollower);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
