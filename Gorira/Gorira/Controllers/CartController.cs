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


        [Authorize(Roles="Member")]
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

                    if (cartVMs == null || !cartVMs.Any(c=>c.UserId == basketVM.AuthorId))
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

                        CartVM cartVM  =  cartVMs.Find(c=>c.UserId == basketVM.AuthorId);
                        List<BasketVM> cartBasketVMs = cartVM.BasketVMs;
                        cartBasketVMs.Add(basketVM);
                        cartVM.BasketVMs = cartBasketVMs;
                    }

                }
            }



            return View(cartVMs);
        }
    }
}
