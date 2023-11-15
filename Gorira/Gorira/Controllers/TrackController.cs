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
    public class TrackController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly int _pageSize;
        public TrackController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _pageSize = 12;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }


        //1.Index
        //2.Upload

        //==========================================

        //1.Index
        public IActionResult Index()
        {
            return View();
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
            track.UserId = appUser.Id;
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
    }
}
