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
using static System.Net.Mime.MediaTypeNames;

namespace Gorira.Controllers
{
    public class TrackController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly int _pageSize;
        private readonly int _myTrackPageSize;
        public TrackController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _pageSize = 9;
            _myTrackPageSize = 5;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }


        //1.Index
        //2.Upload
        //3.Play Counter
        //4.Detail
        //5.My Tracks
        //6.Track Edit
        //7.Track Delete
        //8.Free Download

        //=========================================================

        //1.Index
        public async Task<IActionResult> Index(int? page, string? search, List<int>? genres, List<int>? moods, List<Key>? keys,
            double? minPrice = 0, double? maxPrice = 9999, double? minBpm = 0, double? maxBpm = 9999, string? order = "popular")

        {
            if (page <= 0)
            {
                return NotFound();
            }

            IEnumerable<Track>? Tracks = await _context.Tracks
                .Include(t => t.User)
                .Include(t => t.TrackTags).ThenInclude(tt => tt.Tag)
                .Where(t => t.IsDeleted == false).ToListAsync();



            if (!string.IsNullOrWhiteSpace(search))
            {
                Tracks = Tracks.Where(t => t.Title.ToUpper().Contains(search.Trim().ToUpper()) ||
                t.User.DisplayName.Contains(search.Trim().ToUpper()) ||
                (t.TrackTags != null && t.TrackTags.Any(tt => tt.Tag.Name.Trim().ToUpper().Contains(search.Trim().ToUpper())))
                );
            }


            if (genres != null && genres.Count > 0)
            {
                Tracks = Tracks.Where(t => (t.MainGenreId != null && genres.Contains((int)t.MainGenreId)) ||
                (t.SubGenreId != null && genres.Contains((int)t.SubGenreId)));
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

                "new" => Tracks.OrderByDescending(t => t.CreatedAt),

                _ => Tracks.OrderByDescending(t => t.Plays),

            };



            IPagedList<Track> paginatedTracks = await Tracks.ToPagedListAsync(page ?? 1, _pageSize);


            IEnumerable<Genre> allGenres = await _context.Genres.Where(g => g.IsDeleted == false).OrderBy(g => g.Id).ToArrayAsync();
            IEnumerable<Mood> allMoods = await _context.Moods.Where(g => g.IsDeleted == false).OrderBy(g => g.Id).ToArrayAsync();

            TrackVM trackVM = new TrackVM
            {
                Tracks = paginatedTracks,
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
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> Upload()
        {
            ViewBag.Moods = await _context.Moods.Where(m => m.IsDeleted == false).ToListAsync();
            ViewBag.Genres = await _context.Genres.Where(m => m.IsDeleted == false).ToListAsync();

            return View();
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Track? track, List<string>? TagNames)
        {
            ViewBag.Moods = await _context.Moods.Where(m => m.IsDeleted == false).ToListAsync();
            ViewBag.Genres = await _context.Genres.Where(m => m.IsDeleted == false).ToListAsync();

            if (track == null) return BadRequest();

            if (TagNames == null) return BadRequest();


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

            if (track.UnlimitedPrice != null && track.UnlimitedPrice > 0 && (track.TrackStemsFile == null))
            {
                ModelState.AddModelError("", "If your track's \"unlimited price\" is greater than \"0\", please make sure to include \"Track Stems\" and \"Untagged Audio\"");
                return View(track);
            }

            if ((track.UnlimitedPrice == null || track.UnlimitedPrice <= 0) && track.TrackStemsFile != null)
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

            return RedirectToAction(nameof(MyTracks));
        }

        //3.Play Counter
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> PlayCounter(int? Id)
        {

            if (Id == null) return BadRequest();

            Track? track = await _context.Tracks.FirstOrDefaultAsync(t => t.IsDeleted == false && Id == t.Id);

            if (track == null) return NotFound();

            AppUser? appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            PlayToken? playToken = await _context.PlayTokens.FirstOrDefaultAsync(pt => pt.TrackId == Id && pt.UserId == appUser.Id && pt.IsDeleted == false);

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

                track.Plays++;
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

        //4.Detail
        public async Task<IActionResult> Detail(int? Id)
        {
            if (Id == null) return BadRequest();

            Track? track = await _context.Tracks
                .Include(t => t.User)
                .Include(t => t.PlaylistTracks.Where(pt => pt.IsDeleted == false))
                .Include(t => t.TrackTags.Where(tt => tt.IsDeleted == false)).ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.Id == Id && t.IsDeleted == false);

            if (track == null) return NotFound();

            AppUser? appUser = null;

            IEnumerable<Playlist>? playlists = null;

            if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
            {
                appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                playlists = await _context.Playlists
                    .Include(p => p.User)
                      .Include(p => p.PlaylistFollowers)
                      .Include(p => p.PlaylistTracks)
                    .Where(p => p.IsDeleted == false && (p.UserId == appUser.Id)).ToListAsync();
            }

            IEnumerable<Comment> comments = _context.Comments
               .Include(c => c.User)
               .Where(c => c.IsDeleted == false && c.TrackId == Id).OrderByDescending(c => c.CreatedAt);


            int maxPage = (int)Math.Ceiling((decimal)comments.Count() / 5);
            ViewBag.MaxPage = maxPage;

            TrackDetailVM trackDetailVM = new TrackDetailVM
            {
                track = track,
                playlists = playlists,
                comments = comments.Take(5),

            };

            return View(trackDetailVM);
        }

        //5.My Tracks
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> MyTracks(int? page, string? search)
        {
            if (page <= 0)
            {
                return NotFound();
            }
            ViewBag.Counter = page == null ? 0 : (page - 1) * _myTrackPageSize;
            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            IPagedList<Track>? tracks = await _context.Tracks
                .Include(t => t.User)
                .Where(t => t.IsDeleted == false && t.UserId == appUser.Id).OrderByDescending(c => c.CreatedAt).ToPagedListAsync(page ?? 1, _myTrackPageSize);

            return View(tracks);
        }

        //6.Track Edit

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> Edit(int? Id)
        {
            ViewBag.Moods = await _context.Moods.Where(m => m.IsDeleted == false).ToListAsync();
            ViewBag.Genres = await _context.Genres.Where(m => m.IsDeleted == false).ToListAsync();

            if (Id == null) return BadRequest();

            Track? track = await _context.Tracks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.IsDeleted == false && t.Id == Id);

            if (track == null) return NotFound();


            return View(track);
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Track? track, int? Id, List<string>? TagNames)
        {

            ViewBag.Moods = await _context.Moods.Where(m => m.IsDeleted == false).ToListAsync();
            ViewBag.Genres = await _context.Genres.Where(m => m.IsDeleted == false).ToListAsync();


            if (track == null) return BadRequest();


            if (Id == null) return BadRequest();

            Track DbTrack = await _context.Tracks.FirstOrDefaultAsync(t => t.IsDeleted == false && t.Id == Id);

            if (track.UntaggedFile == null && DbTrack.Untagged == null)
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

            if (track.CoverFile != null)
            {
                if (track.Cover != appUser.ProfilePicture && DbTrack.Cover != null)
                {
                    string filePath = Path.Combine(_env.WebRootPath, "assets", "images", "covers", DbTrack.Cover);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                DbTrack.Cover = await track.CoverFile.Save(_env.WebRootPath, new string[] { "assets", "images", "covers" });
            }

            if (track.TaggedFile != null)
            {
                if (DbTrack.Tagged != null)
                {

                    string filePath = Path.Combine(_env.WebRootPath, "assets", "audio", "tagged", DbTrack.Tagged);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }


                DbTrack.Tagged = await track.TaggedFile.Save(_env.WebRootPath, new string[] { "assets", "audio", "tagged" });
            }

            if (track.UntaggedFile != null)
            {
                if (DbTrack.Untagged != null)
                {
                    string filePath = Path.Combine(_env.WebRootPath, "assets", "audio", "untagged", DbTrack.Untagged);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                DbTrack.Untagged = await track.UntaggedFile.Save(_env.WebRootPath, new string[] { "assets", "audio", "untagged" });
            }

            if (track.TrackStemsFile != null)
            {
                if (DbTrack.TrackStems != null)
                {
                    string filePath = Path.Combine(_env.WebRootPath, "assets", "audio", "stems", DbTrack.TrackStems);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                DbTrack.TrackStems = await track.TrackStemsFile.Save(_env.WebRootPath, new string[] { "assets", "audio", "stems" });
            }


            if (track.UnlimitedPrice != null && track.UnlimitedPrice > 0 && (track.TrackStemsFile == null && DbTrack.TrackStems == null))
            {
                ModelState.AddModelError("", "If your track's \"unlimited price\" is greater than \"0\", please make sure to include \"Track Stems\" and \"Untagged Audio\"");
                return View(track);
            }

            if ((track.UnlimitedPrice == null || track.UnlimitedPrice <= 0) && (track.TrackStemsFile != null || DbTrack.TrackStems != null))
            {
                ModelState.AddModelError("", "If you included \"Track Stems\" and \"Untagged Audio\", please make sure your \"unlimited price\" is greater than \"0\", ");
                return View(track);
            }


            if (track.MainGenreId != null)
            {
                DbTrack.MainGenreId = track.MainGenreId;
            }

            if (track.SubGenreId != null)
            {
                DbTrack.SubGenreId = track.SubGenreId;
            }

            if (track.PrimaryMoodId != null)
            {
                DbTrack.PrimaryMoodId = track.PrimaryMoodId;
            }

            if (track.SecondaryMoodId != null)
            {
                DbTrack.SecondaryMoodId = track.SecondaryMoodId;
            }

            if (track.MusicKey != null)
            {
                DbTrack.MusicKey = track.MusicKey;
            }



            DbTrack.UpdatedBy = appUser.UserName;
            DbTrack.UpdatedAt = DateTime.Now;
            DbTrack.Price = track.Price;
            DbTrack.UnlimitedPrice = track.UnlimitedPrice;
            DbTrack.HasFree = track.HasFree;
            DbTrack.Title = track.Title;
            DbTrack.Bpm = track.Bpm;
            DbTrack.Description = track.Description;

            await _context.SaveChangesAsync();



            if (TagNames.Count == 3)
            {

                List<TrackTag> oldTrackTags = await _context.TrackTags
                    .Include(tt => tt.Tag)
                    .Where(tt => tt.IsDeleted == false && tt.TrackId == Id).ToListAsync();

                foreach (TrackTag oldTrackTag in oldTrackTags)
                {
                    if (oldTrackTag.Tag.Name != TagNames[0] && oldTrackTag.Tag.Name != TagNames[1] && oldTrackTag.Tag.Name != TagNames[2])
                    {
                        oldTrackTag.IsDeleted = true;
                        oldTrackTag.DeletedAt = DateTime.Now;
                        oldTrackTag.DeletedBy = User.Identity.Name;
                    }
                }
                await _context.SaveChangesAsync();

                foreach (string tag in TagNames)
                {
                    if (!await _context.Tags.AnyAsync(t => t.Name.Trim().ToLower() == tag.Trim().ToLower()))
                    {
                        Tag newTag = new Tag
                        {
                            Name = tag,
                            CreatedBy = User.Identity.Name
                        };


                        await _context.Tags.AddAsync(newTag);
                        await _context.SaveChangesAsync();
                        TrackTag trackTag = new TrackTag
                        {
                            TagId = newTag.Id,
                            TrackId = track.Id,
                            CreatedBy = User.Identity.Name

                        };
                        await _context.TrackTags.AddAsync(trackTag);
                        await _context.SaveChangesAsync();
                    }
                    else if (!oldTrackTags.Any(tt => tt.Tag.Name == tag))
                    {
                        Tag existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.IsDeleted == false && t.Name == tag);

                        TrackTag trackTag = new TrackTag
                        {
                            TagId = existingTag.Id,
                            TrackId = track.Id,
                            CreatedBy = User.Identity.Name

                        };
                        await _context.TrackTags.AddAsync(trackTag);
                        await _context.SaveChangesAsync();
                    }
                }

            }


            return RedirectToAction(nameof(MyTracks));
        }

        //7.Track Delete
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> Delete(int? Id)
        {

            if (Id == null) return BadRequest();

            Track? track = await _context.Tracks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.IsDeleted == false && t.Id == Id);

            if (track == null) return NotFound();

            AppUser appUser = await _userManager.Users
           .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (track.UserId != appUser.Id) return NotFound();



            IEnumerable<TrackTag> trackTags = await _context.TrackTags.Where(tt => tt.TrackId == Id && tt.IsDeleted == false).ToListAsync();


            foreach (TrackTag trackTag in trackTags)
            {
                trackTag.IsDeleted = true;
                trackTag.DeletedBy = appUser.UserName;
                trackTag.DeletedAt = DateTime.Now;
            }

            track.IsDeleted = true;
            track.DeletedBy = appUser.UserName;
            track.DeletedAt = DateTime.Now;




            await _context.SaveChangesAsync();


            if (track.Cover != appUser.ProfilePicture && track.Cover != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "images", "covers", track.Cover);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            if (track.Tagged != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "audio", "tagged", track.Tagged);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            if (track.Untagged != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "audio", "untagged", track.Untagged);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            if (track.TrackStems != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "audio", "stems", track.TrackStems);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }





            return RedirectToAction(nameof(MyTracks));
        }

        //8.Free Download
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> FreeDownload(int? Id)
        {
            if (Id == null) return BadRequest();

            Track? track = await _context.Tracks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.IsDeleted == false && t.Id == Id && t.HasFree == true);

            if (track == null) return NotFound();

            string taggedAudio = track.Tagged;

            if (taggedAudio != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "audio", "tagged", track.Tagged);

                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "audio/mpeg", $"{track.User.DisplayName} - {track.Title} (Free Gorira).mp3");
                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
                return NotFound();
            }
        }

        //9.Post Comment
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostComment(int? Id, string? text)
        {
            if (Id == null) return BadRequest();

            if (string.IsNullOrWhiteSpace(text)) return BadRequest();
            if (text.Trim() == "") return BadRequest();

            Track? track = await _context.Tracks.FirstOrDefaultAsync(t => t.IsDeleted == false && t.Id == Id);

            if (track == null) return NotFound();

            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            Comment comment = new Comment
            {
                IsDeleted = false,
                CreatedBy = User.Identity.Name,
                UserId = appUser.Id,
                TrackId = Id,
                Text = text
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            IEnumerable<Comment> comments = _context.Comments
                .Include(c => c.User)
                .Where(c => c.IsDeleted == false && c.TrackId == Id).OrderByDescending(c => c.CreatedAt);

            int maxPage = (int)Math.Ceiling((decimal)comments.Count() / 5);
            ViewBag.MaxPage = maxPage;
            int? PageIndex = TempData["PageIndex"] == null ? 1 : (int)TempData["PageIndex"] + 1;
            if (PageIndex > maxPage) return BadRequest();

            comments = comments.Take((int)PageIndex * 5);

            return PartialView("_CommentsPartial", comments);
        }

        public async Task<IActionResult> LoadMoreComments(int? Id, int? pageIndex)
        {
            if (pageIndex == null) return BadRequest();

            if (pageIndex <= 0) return BadRequest();

            if (Id == null) return BadRequest();

            if (!await _context.Tracks.AnyAsync(t => t.IsDeleted == false && t.Id == Id)) return NotFound();

            IQueryable<Comment> comments = _context.Comments
                  .Include(c => c.User)
                  .Where(c => c.IsDeleted == false && c.TrackId == Id)
                  .OrderByDescending(c => c.CreatedAt);



            int maxPage = (int)Math.Ceiling((decimal)comments.Count() / 5);
            ViewBag.MaxPage = maxPage;
            if (pageIndex > maxPage) return BadRequest();
            TempData["PageIndex"] = pageIndex;
            comments = comments.Skip((int)pageIndex * 5).Take(5);

            return PartialView("_CommentsPartial", new List<Comment>(comments));
        }
    }
}
