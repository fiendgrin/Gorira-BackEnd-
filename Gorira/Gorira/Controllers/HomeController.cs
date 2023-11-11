using Gorira.DataAccessLayer;
using Gorira.ViewModels.HomeVMs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gorira.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            HomeVM homeVMs = new HomeVM
            {
                Settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value)

            };

            return View(homeVMs);
        }
    }
}
