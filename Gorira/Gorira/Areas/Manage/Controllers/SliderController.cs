using Gorira.Areas.Manage.ViewModels.SliderVMs;
using Gorira.DataAccessLayer;
using Gorira.Helpers;
using Gorira.Models;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gorira.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "SuperAdmin, Admin")]

    public class SliderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        public SliderController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }


        public async Task<IActionResult> Index()
        {

            SliderVM sliderVM = new SliderVM
            {
                Sliders = await _context.Sliders.Where(s => s.IsDeleted == false).ToListAsync(),
                ReviewSliders = await _context.ReviewSliders.Where(s => s.IsDeleted == false).ToListAsync(),

            };

            return View(sliderVM);
        }

        public async Task<IActionResult> Detail(int? Id)
        {
            if (Id == null) return BadRequest();

            Slider? slider = await _context.Sliders.FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == Id);

            if (slider == null) return NotFound();

            return View(slider);
        }

        public async Task<IActionResult> ReviewDetail(int? Id)
        {
            if (Id == null) return BadRequest();

            ReviewSlider? reviewSlider = await _context.ReviewSliders.FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == Id);

            if (reviewSlider == null) return NotFound();

            return View(reviewSlider);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Slider slider)
        {
            if (!ModelState.IsValid) return View(slider);


            if (slider.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Image is required");
                return View(slider);

            }

            slider.Image = await slider.ImageFile.Save(_env.WebRootPath, new string[] { "assets", "images", "slider" });


            await _context.Sliders.AddAsync(slider);
            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ReviewCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewCreate(ReviewSlider reviewSlider)
        {
            if (!ModelState.IsValid) return View(reviewSlider);

            if (reviewSlider.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Image is required");
                return View(reviewSlider);

            }

            if (reviewSlider.BackgroundImageFile == null)
            {
                ModelState.AddModelError("BackgroundImageFile", "Background image is required");
                return View(reviewSlider);

            }

            reviewSlider.Image = await reviewSlider.ImageFile.Save(_env.WebRootPath, new string[] { "assets", "images", "WhyGorira" });

            reviewSlider.BackgroundImage = await reviewSlider.BackgroundImageFile.Save(_env.WebRootPath, new string[] { "assets", "images", "gif" });

            await _context.ReviewSliders.AddAsync(reviewSlider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? Id)
        {
            if (Id == null)
            {
                return BadRequest();
            }

            Slider? slider = await _context.Sliders.FirstOrDefaultAsync(r => r.Id == Id && r.IsDeleted == false);

            if (slider == null)
            {
                return NotFound();
            }
            return View(slider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? Id, Slider slider)
        {

            if (Id == null)
            {
                return BadRequest();
            }

            Slider DbSlider = await _context.Sliders.FirstOrDefaultAsync(r => r.Id == Id && r.IsDeleted == false);

            if (DbSlider == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(slider);


            if (slider.ImageFile != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "images", "slider", DbSlider.Image);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                DbSlider.Image = await slider.ImageFile.Save(_env.WebRootPath, new string[] { "assets", "images", "slider" });

            }
            DbSlider.Text = slider.Text;
            DbSlider.BtnText = slider.BtnText;
            DbSlider.Link = slider.Link;

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ReviewUpdate(int? Id)
        {
            if (Id == null)
            {
                return BadRequest();
            }

            ReviewSlider? reviewSlider = await _context.ReviewSliders.FirstOrDefaultAsync(r => r.Id == Id && r.IsDeleted == false);

            if (reviewSlider == null)
            {
                return NotFound();
            }
            return View(reviewSlider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewUpdate(int? Id, ReviewSlider reviewSlider)
        {

            if (Id == null)
            {
                return BadRequest();
            }

            ReviewSlider DbReviewSlider = await _context.ReviewSliders.FirstOrDefaultAsync(r => r.Id == Id && r.IsDeleted == false);

            if (DbReviewSlider == null)
            {
                return NotFound();
            }


            if (!ModelState.IsValid) return View(reviewSlider);


            if (reviewSlider.ImageFile != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "images", "WhyGorira", DbReviewSlider.Image);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                DbReviewSlider.Image = await reviewSlider.ImageFile.Save(_env.WebRootPath, new string[] { "assets", "images", "WhyGorira" });

            }

            if (reviewSlider.BackgroundImageFile != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "images", "gif", DbReviewSlider.BackgroundImage);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                DbReviewSlider.BackgroundImage = await reviewSlider.BackgroundImageFile.Save(_env.WebRootPath, new string[] { "assets", "images", "gif" });
            }

            DbReviewSlider.Text = reviewSlider.Text;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return BadRequest();

            Slider? slider = await _context.Sliders.FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == Id);

            if (slider == null) return NotFound();

            return View(slider);
        }

        public async Task<IActionResult> DeleteSlider(int? Id)
        {
            if (Id == null) return BadRequest();

            Slider? slider = await _context.Sliders.FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == Id);

            if (slider == null) return NotFound();


            if (slider.Image != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "images", "slider", slider.Image);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

            }

            slider.DeletedBy = User.Identity.Name;
            slider.DeletedAt = DateTime.Now;
            slider.IsDeleted = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ReviewDelete(int? Id)
        {
            if (Id == null) return BadRequest();

            ReviewSlider? reviewSlider = await _context.ReviewSliders.FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == Id);

            if (reviewSlider == null) return NotFound();

            return View(reviewSlider);
        }

        public async Task<IActionResult> DeleteReviewSlider(int? Id)
        {
            if (Id == null) return BadRequest();

            ReviewSlider? reviewSlider = await _context.ReviewSliders.FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == Id);

            if (reviewSlider == null) return NotFound();


            if (reviewSlider.Image != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "images", "WhyGorira", reviewSlider.Image);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

            }

            if (reviewSlider.BackgroundImage != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "images", "gif", reviewSlider.BackgroundImage);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

            }

            reviewSlider.DeletedBy = User.Identity.Name;
            reviewSlider.DeletedAt = DateTime.Now;
            reviewSlider.IsDeleted = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
