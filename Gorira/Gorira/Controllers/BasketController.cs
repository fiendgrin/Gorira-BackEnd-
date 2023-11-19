using Gorira.DataAccessLayer;
using Gorira.Models;
using Gorira.ViewModels.BasketVMs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Gorira.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> AddBasket(int? id, bool? isUnlimited)
        {
            if (id == null) return BadRequest();

            if (isUnlimited == null) return BadRequest();

            if (!await _context.Tracks.AnyAsync(p => p.IsDeleted == false && p.Id == id)) return NotFound();

            if (await _context.Tracks.AnyAsync(p => p.IsDeleted == false && p.Id == id && isUnlimited == true && p.UnlimitedPrice == null)) return BadRequest();


            string? basket = Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;
            if (!string.IsNullOrWhiteSpace(basket))
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

                if (!basketVMs.Exists(b => b.Id == id))
                {
                    basketVMs.Add(new BasketVM
                    {
                        Id = (int)id,
                        IsUnlimited = (bool)isUnlimited,
                    });
                }

            }
            else
            {

                basketVMs = new List<BasketVM> { new BasketVM
                {
                    Id = (int)id,
                   IsUnlimited = (bool)isUnlimited,
                }
            };

            }

            basket = JsonConvert.SerializeObject(basketVMs);

            Response.Cookies.Append("basket", basket);

            if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
            {
                AppUser appUser = await _userManager.Users
                    .Include(b => b.Baskets.Where(b => b.IsDeleted == false))
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (appUser != null && appUser.Baskets != null && appUser.Baskets.Count() > 0)
                {
                    Basket? userBasket = appUser.Baskets.FirstOrDefault(b => b.TrackId == id);
                    if (userBasket == null)
                    {
                        Basket userNewBasket = new Basket
                        {
                            UserId = appUser.Id,
                            TrackId = id,
                            IsUnlimited = basketVMs.FirstOrDefault(b => b.Id == id).IsUnlimited
                        };

                        await _context.AddAsync(userNewBasket);
                    }
                

                }
                else
                {
                    Basket userNewBasket = new Basket
                    {
                        UserId = appUser.Id,
                        TrackId = id,
                        IsUnlimited = basketVMs.FirstOrDefault(b => b.Id == id).IsUnlimited
                    };

                    await _context.AddAsync(userNewBasket);
                }
                await _context.SaveChangesAsync();
            }

            foreach (BasketVM basketVM in basketVMs)
            {
                Track track = await _context.Tracks
                  .Include(t => t.User)
                  .FirstOrDefaultAsync(t => t.Id == basketVM.Id);
                basketVM.Title = track.Title;
                basketVM.Id = track.Id;
                basketVM.Image = track.Cover;
                basketVM.Price = basketVM.IsUnlimited ? track.UnlimitedPrice : track.Price;
                basketVM.AuthorPfp = track.User.ProfilePicture;
                basketVM.AuthorId = track.UserId;
                basketVM.AuthorName = track.User.DisplayName;

            }


            return PartialView("_BasketPartial", basketVMs);
        }

        public async Task<IActionResult> RemoveBasket(int? id)
        {
            if (id == null) return BadRequest();

            string? basket = Request.Cookies["basket"];

            List<BasketVM>? basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);


            if (!basketVMs.Any(t => t.Id == id)) return NotFound();

            basketVMs.RemoveAll(t => t.Id == id);

            basket = JsonConvert.SerializeObject(basketVMs);

            Response.Cookies.Append("basket", basket);


            if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
            {
                AppUser appUser = await _userManager.Users
                    .Include(b => b.Baskets.Where(b => b.IsDeleted == false))
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (appUser != null)
                {
                    Basket? userBasket = appUser.Baskets?.FirstOrDefault(b => b.TrackId == id);


                    if (userBasket != null)
                    {
                        userBasket.DeletedAt = DateTime.Now;
                        userBasket.IsDeleted = true;
                        userBasket.DeletedBy = User.Identity.Name;
                    }

                }
                await _context.SaveChangesAsync();
            }

            foreach (BasketVM basketVM in basketVMs)
            {

                Track track = await _context.Tracks
                  .Include(p => p.User)
                  .FirstOrDefaultAsync(p => p.Id == basketVM.Id);
                basketVM.Title = track.Title;
                basketVM.Id = track.Id;
                basketVM.Image = track.Cover;
                basketVM.Price = basketVM.IsUnlimited ? track.UnlimitedPrice : track.Price;
                basketVM.AuthorPfp = track.User.ProfilePicture;
                basketVM.AuthorId = track.UserId;
                basketVM.AuthorName = track.User.DisplayName;


            }



            return PartialView("_BasketPartial", basketVMs);
        }

    }
}
