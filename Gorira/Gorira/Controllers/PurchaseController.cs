using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Gorira.DataAccessLayer;
using Gorira.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Gorira.Attributes.ValidationAttributes;
using System.IO.Compression;

namespace Gorira.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly int _pageSize;
        public PurchaseController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _pageSize = 10;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> Index(int ? page)
        {
            AppUser appUser = await _userManager.Users
               .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            IPagedList<Purchase>? purchases = await _context.Purchases
                .Include(p => p.Track).ThenInclude(t=>t.User)
                .Where(p => p.UserId == appUser.Id && p.IsDeleted == false && (p.Track != null && p.Track.IsDeleted == false)).ToPagedListAsync(page ?? 1, _pageSize);

            return View(purchases);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> Checkout()
        {
           AppUser appUser = await _userManager.Users
                 .Include(u => u.Baskets.Where(b => b.IsDeleted == false))
                .FirstOrDefaultAsync(u=>u.UserName == User.Identity.Name);

            IEnumerable<Basket>? baskets = await _context.Baskets
                .Include(b=>b.Track)
                .Where(b => b.UserId == appUser.Id && b.IsDeleted == false).ToListAsync();

            if (appUser.Baskets == null || appUser.Baskets.Count() <= 0)
            {

                TempData["Warning"] = "Add tracks to basket before checking out";
                return RedirectToAction("Index", "Track");
            }

            List<Purchase> purchases = new List<Purchase>();
            IEnumerable<Purchase> existingPurchases = await _context.Purchases
                .Where(p => p.UserId == appUser.Id && p.IsDeleted == false).ToListAsync();


            foreach (Basket basket in baskets)
            {
                if (existingPurchases.Any(p => p.TrackId == basket.TrackId && p.IsUnlimited == basket.IsUnlimited))
                {
                    TempData["Warning"] = "One or more tracks in your cart have already been purchased with the same license";
                    return RedirectToAction("Index", "Cart");
                }
                else if (existingPurchases.Any(p => p.TrackId == basket.TrackId && p.IsUnlimited ==true && basket.IsUnlimited == false)) 
                {
                    TempData["Warning"] = "You have already purchased a higher license for one of the tracks in your cart";
                    return RedirectToAction("Index", "Cart");
                }
                Purchase purchase = new Purchase
                {
                    UserId = appUser.Id,
                    TrackId = basket.TrackId,
                    Price = basket.IsUnlimited ? basket.Track.UnlimitedPrice : basket.Track.Price,
                    IsUnlimited = basket.IsUnlimited,
                };

                basket.IsDeleted = true;
                basket.DeletedAt = DateTime.Now;
                basket.DeletedBy = appUser.UserName;

                purchases.Add(purchase);
            }

            await _context.Purchases.AddRangeAsync(purchases);
            await _context.SaveChangesAsync();

            Response.Cookies.Append("basket", "");

            TempData["Success"] = "Your Purchase Is Successful";

            return RedirectToAction(nameof(Index));
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> Download(int? Id) 
        {
            if (Id == null) return BadRequest();

            AppUser appUser = await _userManager.Users
              .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

           Purchase? purchase = await _context.Purchases
                .Include(p => p.Track)
                .FirstOrDefaultAsync(p =>p.Id == Id &&  p.UserId == appUser.Id && p.IsDeleted == false && (p.Track != null && p.Track.IsDeleted == false));
            
            if (purchase == null) return NotFound();

            Track? track = await _context.Tracks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.IsDeleted == false && t.Id == purchase.TrackId);

            if (track == null) return NotFound();


            string taggedAudio = track.Tagged;
            string audio = track.Untagged;
            string zipOrRar = track.TrackStems;

            string taggedAudioFilePath = Path.Combine(_env.WebRootPath, "assets", "audio", "tagged", track.Tagged);
            string audioFilePath = Path.Combine(_env.WebRootPath, "assets", "audio", "untagged", track.Untagged);
            string zipOrRarFilePath = Path.Combine(_env.WebRootPath, "assets", "audio", "stems", track.TrackStems);

            string tempZipFile = Path.Combine(_env.WebRootPath,"assets" , "temp", $"files-{appUser.UserName}{track.Id}.zip");

            using (ZipArchive archive = ZipFile.Open(tempZipFile, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(taggedAudioFilePath, $"{track.User.DisplayName} - {track.Title} (tagged Gorira).mp3");

                // Determine the file extension for untagged audio (mp3 or wav)
                string untaggedAudioExtension = Path.GetExtension(audioFilePath).ToLower();
                string untaggedAudioFileName = untaggedAudioExtension == ".mp3" ? $"{track.User.DisplayName} - {track.Title} (untagged Gorira).mp3" : $"{track.User.DisplayName} - {track.Title} (untagged Gorira).wav";

                archive.CreateEntryFromFile(audioFilePath, untaggedAudioFileName);

                if (purchase.IsUnlimited)
                {
                    // Determine the file extension for track stems (zip or rar)
                    string trackStemsExtension = Path.GetExtension(zipOrRarFilePath).ToLower();
                    string trackStemsFileName = trackStemsExtension == ".zip" ? $"{track.User.DisplayName} - {track.Title} (stems Gorira).zip" : $"{track.User.DisplayName} - {track.Title} (stems Gorira).rar";
                    archive.CreateEntryFromFile(zipOrRarFilePath, trackStemsFileName);
                }
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(tempZipFile);
            System.IO.File.Delete(tempZipFile);

            return File(fileBytes, "application/zip", $"{track.Title}(Gorira).zip");
        }


    }
}
