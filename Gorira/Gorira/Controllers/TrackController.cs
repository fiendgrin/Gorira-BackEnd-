using Gorira.DataAccessLayer;
using Gorira.Enums;
using Gorira.Helpers;
using Gorira.Models;
using Gorira.ViewModels.ArtistVMs;
using Gorira.ViewModels.TrackVMs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;
using X.PagedList;

namespace Gorira.Controllers
{
    public class TrackController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly int _pageSize;
        public TrackController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _pageSize = 9;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }


        //1.Index
        //2.Upload

        //=========================================================

        //1.Index
        public async Task<IActionResult> Index(int? page, string? search, List<int>? genres,List<int>? moods,List<Key>? keys,
            double? minPrice = 0,double? maxPrice = 9999, double? minBpm = 0, double? maxBpm=9999, string? order = "popular")
        
        {
            if (page <= 0)
            {
                return NotFound();
            }

            AppUser? appUser = null;

            if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
            {
                appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            }

            IEnumerable<Track>? Tracks = await _context.Tracks
                .Where(t => t.IsDeleted == false && (appUser == null || t.UserId != appUser.Id)).ToListAsync();

          

            if (!string.IsNullOrWhiteSpace(search))
            {
                Tracks = Tracks.Where(t => t.Title.ToUpper().Contains(search.Trim().ToUpper()));
            }


            if (genres != null && genres.Count > 0)
            {
                Tracks = Tracks.Where(t => (t.MainGenreId != null && genres.Contains((int)t.MainGenreId)) || 
                (t.SubGenreId != null && genres.Contains((int)t.SubGenreId)) );
            }

            if (moods != null && moods.Count > 0)
            {
                Tracks = Tracks.Where(t => (t.PrimaryMoodId != null && moods.Contains((int)t.PrimaryMoodId)) ||
                (t.SecondaryMoodId != null && moods.Contains((int)t.SecondaryMoodId)));
            }

            if (keys != null && keys.Count > 0)
            {
                Tracks = Tracks.Where(t => keys.Contains(t.MusicKey));
            }


            if (minBpm != 0 || maxBpm != 9999)
            {
                if (minBpm == null)
                {
                    minBpm = 0;
                }
                if (maxBpm == null)
                {
                    maxBpm = 9999;
                }
                Tracks = Tracks.Where(t => t.Bpm >= minBpm && t.Bpm <= maxBpm);
            }

            if (minPrice != 0 || maxPrice != 9999)
            {
                if (minPrice == null)
                {
                    minPrice = 0;
                }
                if (maxPrice == null)
                {
                    maxPrice = 9999;
                }

                Tracks = Tracks.Where(t => t.Price >= minPrice && t.Price <= maxPrice);
            }



            Tracks = order switch
            {
                "popular" => Tracks.OrderByDescending(t => t.Plays),

                "A-Z" => Tracks.OrderBy(t => t.Title),

                "new" => Tracks.OrderByDescending(t => t.CreatedAt)

            };



            IPagedList<Track> paginatedTracks = await Tracks.ToPagedListAsync(page ?? 1, _pageSize);


            List<AppUser> allUsers = await _userManager.Users.Where(u => u.IsActive == true).ToListAsync();
            IEnumerable<Genre> allGenres = await _context.Genres.Where(g => g.IsDeleted == false).OrderBy(g=>g.Id).ToArrayAsync();
            IEnumerable<Mood> allMoods = await _context.Moods.Where(g => g.IsDeleted == false).OrderBy(g => g.Id).ToArrayAsync();

          TrackVM  trackVM = new TrackVM
            {
                Tracks = paginatedTracks,
                Users = allUsers,
               filterVM = new FilterVM 
               {
                   Genres = allGenres,
                   Moods = allMoods,
                   selectedGenres = genres,
                   selectedMoods = moods,
                   selectedKeys = keys,
                   minBpm = minBpm,
                   maxBpm = maxBpm,
                   minPrice = minPrice,
                   maxPrice = maxPrice,
               }
            };

            return View(trackVM);
        }

        //2.Upload
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Upload()
        {
            ViewBag.Moods = await _context.Moods.Where(m => m.IsDeleted == false).ToListAsync();
            ViewBag.Genres = await _context.Genres.Where(m => m.IsDeleted == false).ToListAsync();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Track track, List<string> TagNames)
        {
            ViewBag.Moods = await _context.Moods.Where(m => m.IsDeleted == false).ToListAsync();
            ViewBag.Genres = await _context.Genres.Where(m => m.IsDeleted == false).ToListAsync();

            if (TagNames.Count != 3)
            {
                ModelState.AddModelError("", "3 tags required");
                return View("Upload", track);
            }

            if (track.TaggedFile == null)
            {
                ModelState.AddModelError("", "Tagged Audio is required");
                return View(track);
            }

            if (track.UntaggedFile == null)
            {
                ModelState.AddModelError("", "Untagged Audio is required");
                return View(track);
            }


            if (!ModelState.IsValid)
            {
                return View(track);
            }


            AppUser appUser = await _userManager.Users
           .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (track.CoverFile == null)
            {
                track.Cover = appUser.ProfilePicture;
            }
            else
            {

                track.Cover = await track.CoverFile.Save(_env.WebRootPath, new string[] { "assets", "images", "covers" });
            }

            track.Tagged = await track.TaggedFile.Save(_env.WebRootPath, new string[] { "assets", "audio", "tagged" });



            if (track.UntaggedFile != null)
            {
                track.Untagged = await track.UntaggedFile.Save(_env.WebRootPath, new string[] { "assets", "audio", "untagged" });
            }

            if (track.TrackStemsFile != null)
            {
                track.TrackStems = await track.TrackStemsFile.Save(_env.WebRootPath, new string[] { "assets", "audio", "stems" });
            }

            if (track.UnlimitedPrice != null && track.UnlimitedPrice > 0 && (track.TrackStemsFile == null || track.UntaggedFile == null))
            {
                ModelState.AddModelError("", "If your track's \"unlimited price\" is greater than \"0\", please make sure to include \"Track Stems\" and \"Untagged Audio\"");
                return View(track);
            }

            if ((track.UnlimitedPrice == null || track.UnlimitedPrice <= 0) && track.TrackStemsFile != null && track.UntaggedFile != null)
            {
                ModelState.AddModelError("", "If you included \"Track Stems\" and \"Untagged Audio\", please make sure your \"unlimited price\" is greater than \"0\", ");
                return View(track);
            }

            if (track.MainGenreId == null)
            {
                ModelState.AddModelError("MainGenreId", "Main Genre Is required");
                return View(track);
            }


            track.CreatedBy = appUser.UserName;
            track.IsDeleted = false;
            track.UserId = appUser.Id;
            track.Plays = 0;
            await _context.Tracks.AddAsync(track);
            await _context.SaveChangesAsync();


            foreach (string tag in TagNames)
            {
                if (!await _context.Tags.AnyAsync(t => t.Name.Trim().ToLower() == tag.Trim().ToLower() && t.IsDeleted == false))
                {
                    Tag newTag = new Tag
                    {
                        Name = tag,
                    };


                    await _context.Tags.AddAsync(newTag);
                    await _context.SaveChangesAsync();
                    TrackTag trackTag = new TrackTag
                    {
                        TagId = newTag.Id,
                        TrackId = track.Id

                    };
                    await _context.TrackTags.AddAsync(trackTag);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    Tag existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.IsDeleted == false && t.Name == tag);

                    TrackTag trackTag = new TrackTag
                    {
                        TagId = existingTag.Id,
                        TrackId = track.Id

                    };
                    await _context.TrackTags.AddAsync(trackTag);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index", "Home");
        }

        
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> PlayCounter(int? Id) 
        {

            if (Id == null) return BadRequest();

            Track? track = await _context.Tracks.FirstOrDefaultAsync(t => t.IsDeleted == false && Id == t.Id);

            if (track == null) return NotFound();

            AppUser? appUser = await _userManager.Users.FirstOrDefaultAsync(u=>u.UserName == User.Identity.Name);

            PlayToken? playToken = await _context.PlayTokens.FirstOrDefaultAsync(pt=>pt.TrackId == Id && pt.UserId == appUser.Id && pt.IsDeleted == false);

            if (playToken == null)
            {
                playToken = new PlayToken
                {
                    UserId = appUser.Id,
                    TrackId = Id,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = appUser.UserName,
                };

                await _context.PlayTokens.AddAsync(playToken);

                track.Plays = 1;
            }
            else 
            {
                TimeSpan? timeDifference = DateTime.UtcNow - playToken.UpdatedAt;

                if (timeDifference.Value.TotalHours >= 1)
                {
                    track.Plays++;
                    playToken.UpdatedAt = DateTime.UtcNow;
                }
            }
            await _context.SaveChangesAsync();


            return Ok(playToken);
        }
    }
}
