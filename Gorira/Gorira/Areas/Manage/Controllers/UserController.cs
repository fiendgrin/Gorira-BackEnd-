using Gorira.Areas.Manage.ViewModels.UserVMs;
using Gorira.DataAccessLayer;
using Gorira.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using X.PagedList;

namespace Gorira.Areas.Manage.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "SuperAdmin, Admin")]
    [Area("Manage")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly int _pageSize;
        private readonly int _detailPageSize;


        public UserController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _pageSize = 10;
            _detailPageSize = 5;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(int? page)
        {
            if (page <= 0)
            {
                return NotFound();
            }
            ViewBag.Counter = page == null ? 0 : (page - 1) * _pageSize;
            ViewData["page"] = page;
            IPagedList<AppUser> users = await _context.Users
                .Include(u=>u.Reports.Where(r=>r.IsDeleted == false))
                .Where(u => u.UserName != User.Identity.Name).ToPagedListAsync(page ?? 1, _pageSize); ;

            foreach (var user in users)
            {
                user.Roles = await _userManager.GetRolesAsync(user);
            }

            return View(users);
        }

        public async Task<IActionResult> Detail(int? page,string? Id)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                return BadRequest();
            }

            AppUser? appUser = await _context.Users
                .Include(u => u.Tracks.Where(a => a.IsDeleted == false))
                .Include(u => u.Followers.Where(a => a.IsDeleted == false))
                .FirstOrDefaultAsync(u => u.Id == Id);

            if (appUser == null)
            {
                return NotFound();
            }

            IPagedList<Track>? tracks = await _context.Tracks
                .Include(t=>t.MainGenre)
                .Include(t=>t.SubGenre)
                .Where(t=>t.UserId == Id && t.IsDeleted == false).ToPagedListAsync(page ?? 1, _detailPageSize);

           
            if (appUser == null)
            {
                return NotFound();
            }
            appUser.Roles = await _userManager.GetRolesAsync(appUser);

            UserDetailVM userDetailVM = new UserDetailVM 
            {
                AppUser = appUser,
                Tracks = tracks
            };

            return View(userDetailVM);
        }

        public async Task<IActionResult> SetActive(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            bool active = appUser.IsActive;

            appUser.IsActive = !active;

            await _userManager.UpdateAsync(appUser);

            return RedirectToAction("Index","User",new {page = ViewData["page"] });
        }

        public async Task<IActionResult> ResetPassword(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);

            await _userManager.ResetPasswordAsync(appUser, token, "GoriraSecretPassword123");

            return RedirectToAction("Index", "User", new { page = ViewData["page"] });
        }

        public async Task<IActionResult> ChangeRole(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            string AuthenticatedId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (id == AuthenticatedId) return BadRequest();

            AppUser appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (appUser == null) return NotFound();


            appUser.Roles = await _userManager.GetRolesAsync(appUser);

            List<string> roles = new List<string>();

            foreach (var item in _roleManager.Roles.ToList())
            {
                roles.Add(item.ToString());
            }

            ViewBag.Roles = roles;

            return View(appUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(AppUser appUser)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _roleManager.Roles.ToList();
                return View(appUser);
            }
            AppUser dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == appUser.Id);

            List<string> AllNewRoles = new List<string>();
            AllNewRoles.AddRange(appUser.Roles);

            await _userManager.RemoveFromRolesAsync(dbUser, await _userManager.GetRolesAsync(dbUser));
            await _userManager.AddToRolesAsync(dbUser, AllNewRoles);

            IdentityResult identityResult = await _userManager.UpdateAsync(dbUser);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);

                }
                return View(dbUser);
            }


            return RedirectToAction(nameof(Index));
        }
    }
}
