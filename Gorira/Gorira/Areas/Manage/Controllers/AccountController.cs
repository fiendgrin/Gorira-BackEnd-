using Gorira.Areas.Manage.ViewModels.AccountVMs;
using Gorira.DataAccessLayer;
using Gorira.Models;
using Gorira.ViewModels.AccountVMs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Gorira.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class AccountController : Controller
    {

        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
            SignInManager<AppUser> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
        }


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(ViewModels.AccountVMs.LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }

            AppUser appUser = await _userManager.FindByEmailAsync(loginVM.Email);

            if (appUser == null)
            {
                ModelState.AddModelError("", "Email or Password are incorrect");
                return View(loginVM);
            }

            //if (!appUser.IsActive)
            //{
            //    return Unauthorized();
            //}

            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager
                .PasswordSignInAsync(appUser, loginVM.Password, loginVM.RememberMe, true);


            if (appUser.LockoutEnd != null && (appUser.LockoutEnd - DateTime.Now).Value.Minutes > 0)
            {
                int date = (appUser.LockoutEnd - DateTime.Now).Value.Minutes;

                ModelState.AddModelError("", $"Your Account is blocked ({date} minutes left)");
                return View(loginVM);
            }


            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError("", "Email or password are incorrect");
                return View(loginVM);
            }


            return RedirectToAction("Index", "Dashboard", new { area = "manage" });
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Dashboard");
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> AdminEditProfile()
        {
            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            AdminEditProfileVM adminEditProfileVM = new AdminEditProfileVM
            {
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
            };

            return View(adminEditProfileVM);
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "SuperAdmin, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminEditProfile(AdminEditProfileVM adminEditProfileVM)
        {

            AppUser DbAppUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return View(adminEditProfileVM);
            }

            if (adminEditProfileVM.CurrentPassword != null && adminEditProfileVM.NewPassword != null && adminEditProfileVM.ConfirmPassword != null)
            {
                bool PasswordCheck = await _userManager.CheckPasswordAsync(DbAppUser, adminEditProfileVM.CurrentPassword);

                if (!PasswordCheck)
                {
                    ModelState.AddModelError("", "Incorrect Password");
                    return View(adminEditProfileVM);

                }

                IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(DbAppUser, adminEditProfileVM.CurrentPassword, adminEditProfileVM.NewPassword);

                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(adminEditProfileVM);
                }
            }


            DbAppUser.FirstName = adminEditProfileVM.FirstName;
            DbAppUser.LastName = adminEditProfileVM.LastName;

            IdentityResult identityResult = await _userManager.UpdateAsync(DbAppUser);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);

                }
                return View("AccountSettings", adminEditProfileVM);
            }


            await _signInManager.SignInAsync(DbAppUser, true);

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
