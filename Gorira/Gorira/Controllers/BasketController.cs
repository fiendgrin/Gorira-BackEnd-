using Gorira.DataAccessLayer;
using Gorira.Models;
using Gorira.ViewModels.BasketVMs;
using Microsoft.AspNetCore.Authorization;
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

        //1.Index
        //2.AddBasket
        //3.RemoveBasket

        //================================================

        //1.Index
        public IActionResult Index()
        {
            return View();
        }

        //2.AddBasket
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> AddBasket(int? Id, bool? isUnlimited)
        {
            if (Id == null) return BadRequest();

            if (isUnlimited == null) return BadRequest();

            Track? trackCheck = await _context.Tracks.FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == Id);

            if (trackCheck == null) return NotFound();

            if (await _context.Tracks.AnyAsync(p => p.IsDeleted == false && p.Id == Id && isUnlimited == true && p.UnlimitedPrice == null)) return BadRequest();


            AppUser appUser = await _userManager.Users
                   .Include(b => b.Baskets.Where(b => b.IsDeleted == false))
                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (trackCheck.UserId == appUser.Id) return Conflict();

            string? basket = Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;
            if (!string.IsNullOrWhiteSpace(basket))
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

                if (!basketVMs.Exists(b => b.Id == Id))
                {
                    basketVMs.Add(new BasketVM
                    {
                        Id = (int)Id,
                        IsUnlimited = (bool)isUnlimited,
                    });
                }

            }
            else
            {

                basketVMs = new List<BasketVM> { new BasketVM
                {
                    Id = (int)Id,
                   IsUnlimited = (bool)isUnlimited,
                }
            };

            }

            basket = JsonConvert.SerializeObject(basketVMs);

            Response.Cookies.Append("basket", basket);

           
               

                if (appUser != null && appUser.Baskets != null && appUser.Baskets.Count() > 0)
                {
                    Basket? userBasket = appUser.Baskets.FirstOrDefault(b => b.TrackId == Id);
                    if (userBasket == null)
                    {
                        Basket userNewBasket = new Basket
                        {
                            UserId = appUser.Id,
                            TrackId = Id,
                            IsUnlimited = basketVMs.FirstOrDefault(b => b.Id == Id).IsUnlimited
                        };

                        await _context.AddAsync(userNewBasket);
                    }
                

                }
                else
                {
                    Basket userNewBasket = new Basket
                    {
                        UserId = appUser.Id,
                        TrackId = Id,
                        IsUnlimited = basketVMs.FirstOrDefault(b => b.Id == Id).IsUnlimited
                    };

                    await _context.AddAsync(userNewBasket);
                }
                await _context.SaveChangesAsync();
           

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

        //3.RemoveBasket
        [Microsoft.AspNetCore.Authorization.Authorize(Roles ="Member")]
        public async Task<IActionResult> RemoveBasket(int? Id)
        {
            if (Id == null) return BadRequest();

            string? basket = Request.Cookies["basket"];

            List<BasketVM>? basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);


            if (!basketVMs.Any(t => t.Id == Id)) return NotFound();

            basketVMs.RemoveAll(t => t.Id == Id);

            basket = JsonConvert.SerializeObject(basketVMs);

            Response.Cookies.Append("basket", basket);


                AppUser appUser = await _userManager.Users
                    .Include(b => b.Baskets.Where(b => b.IsDeleted == false))
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (appUser != null)
                {
                    Basket? userBasket = appUser.Baskets?.FirstOrDefault(b => b.TrackId == Id);


                    if (userBasket != null)
                    {
                        userBasket.DeletedAt = DateTime.Now;
                        userBasket.IsDeleted = true;
                        userBasket.DeletedBy = User.Identity.Name;
                    }

                }
                await _context.SaveChangesAsync();
          

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
