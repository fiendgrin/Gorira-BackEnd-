using Gorira.DataAccessLayer;
using Gorira.Models;
using Gorira.ViewModels.BasketVMs;
using Gorira.ViewModels.CartVMs;
using Gorira.ViewModels.TrackVMs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Gorira.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;


        public CartController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //1.Index
        //2.Remove Cart
        //===============================================

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> Index()
        {
            List<CartVM>? cartVMs = new List<CartVM>();
            List<BasketVM> basketVMs = new List<BasketVM>();
            string? basket = Request.Cookies["basket"];
            if (string.IsNullOrWhiteSpace(basket))
            {
                return View(basketVMs);
            }
            else
            {

                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);
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

                    if (cartVMs == null || !cartVMs.Any(c => c.UserId == basketVM.AuthorId))
                    {
                        List<BasketVM> cartBasketVMs = new List<BasketVM>
                        {
                            basketVM
                        };
                        CartVM cartVM = new CartVM
                        {
                            BasketVMs = cartBasketVMs,
                            UserId = basketVM.AuthorId,
                        };

                        cartVMs.Add(cartVM);
                    }
                    else
                    {

                        CartVM cartVM = cartVMs.Find(c => c.UserId == basketVM.AuthorId);
                        List<BasketVM> cartBasketVMs = cartVM.BasketVMs;
                        cartBasketVMs.Add(basketVM);
                        cartVM.BasketVMs = cartBasketVMs;
                    }

                }
            }



            return View(cartVMs);
        }

        //2.Remove Cart
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> RemoveCart(int? Id)
        {
            if (Id == null) return BadRequest();

            string? basket = Request.Cookies["basket"];

            List<BasketVM>? basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

            AppUser appUser = await _userManager.Users
                  .Include(b => b.Baskets.Where(b => b.IsDeleted == false))
                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (!basketVMs.Any(t => t.Id == Id)) return NotFound();

            if (!appUser.Baskets.Any(b=>b.Id != Id)) return NotFound();


            basketVMs.RemoveAll(t => t.Id == Id);

            basket = JsonConvert.SerializeObject(basketVMs);

            Response.Cookies.Append("basket", basket);



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


            List<CartVM>? cartVMs = new List<CartVM>();

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

                if (cartVMs == null || !cartVMs.Any(c => c.UserId == basketVM.AuthorId))
                {
                    List<BasketVM> cartBasketVMs = new List<BasketVM>
                        {
                            basketVM
                        };
                    CartVM cartVM = new CartVM
                    {
                        BasketVMs = cartBasketVMs,
                        UserId = basketVM.AuthorId,
                    };

                    cartVMs.Add(cartVM);
                }
                else
                {

                    CartVM cartVM = cartVMs.Find(c => c.UserId == basketVM.AuthorId);
                    List<BasketVM> cartBasketVMs = cartVM.BasketVMs;
                    cartBasketVMs.Add(basketVM);
                    cartVM.BasketVMs = cartBasketVMs;
                }


            }

            return PartialView("_CartPartial", cartVMs);
        }
    }
}
