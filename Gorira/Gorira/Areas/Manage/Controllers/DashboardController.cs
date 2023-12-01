using Gorira.DataAccessLayer;
using Gorira.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Gorira.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles ="SuperAdmin, Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly int _pageSize;
        public DashboardController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _pageSize = 10;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }
        public async Task<IActionResult> Index(int? page)
        {
            if (page <= 0)
            {
                return NotFound();
            }

            IPagedList<Purchase> purchases = await _context.Purchases
                .Include(p => p.Track).ThenInclude(t => t.User)
                .Include(p => p.User)
                .Where(p => p.IsDeleted == false).OrderByDescending(p=>p.CreatedAt).ToPagedListAsync(page ?? 1,_pageSize);

            return View(purchases);
        }
    }
}
