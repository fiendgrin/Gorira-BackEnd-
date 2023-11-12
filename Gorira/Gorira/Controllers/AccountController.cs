using Gorira.DataAccessLayer;
using Gorira.Models;
using Gorira.Helpers;
using Gorira.ViewModels;
using Gorira.ViewModels.AccountVMs;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.RegularExpressions;

namespace Gorira.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly SmtpSetting _smtpSetting;
        private readonly IWebHostEnvironment _env;

        public AccountController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
            SignInManager<AppUser> signInManager, IConfiguration config, IOptions<SmtpSetting> options, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _config = config;
            _smtpSetting = options.Value;
            _env = env;
        }

        //1.Register
        //2.Login
        //3.Logout
        //4.Email Confirmation
        //5.Edit Profile
        //6.Change Email
        //7.Change PhoneNumber
        //8.Change Password
        //=======================================================

        //1.Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            AppUser appUser = new AppUser
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                IsActive = true,
                ProfilePicture = "default2.jpg",
                DisplayName = registerVM.UserName
            };

            IdentityResult identityResult = await _userManager.CreateAsync(appUser, registerVM.Password);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }
                return View(registerVM);
            }

            await _userManager.AddToRoleAsync(appUser, "Member");


            string templateFullPath = Path.Combine(_env.WebRootPath, "templates", "EmailConfirm.html");
            string templateContent = await System.IO.File.ReadAllTextAsync(templateFullPath);

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            string url = Url.Action("EmailConfirm", "Account", new { Id = appUser.Id, token = token }, Request.Scheme, Request.Host.ToString());

            templateContent = templateContent.Replace("{{url}}", url);

            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(_smtpSetting.Email));
            mimeMessage.To.Add(MailboxAddress.Parse(appUser.Email));
            mimeMessage.Subject = "Email Confirmation";
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = templateContent
            };

            using (SmtpClient client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpSetting.Host, _smtpSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpSetting.Email, _smtpSetting.Password);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }

            return RedirectToAction(nameof(Login));
        }

        //2.Login
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }

            AppUser appUser = await _userManager.Users
                //.Include(u => u.Baskets.Where(b => b.IsDeleted == false))
                .FirstOrDefaultAsync(u => u.NormalizedEmail == loginVM.Email.Trim().ToUpperInvariant());
            if (appUser == null)
            {
                ModelState.AddModelError("", "Email or Password are Incorrect");
                return View(loginVM);
            }

            IList<string> roles = await _userManager.GetRolesAsync(appUser);

            if (!roles.Any(r => r == "Member"))
            {
                ModelState.AddModelError("", "Email or Password are Incorrect");
                return View(loginVM);
            }

            if (!appUser.EmailConfirmed)
            {
                ModelState.AddModelError("", "Confirm Your Email");
                return View(loginVM);
            }

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
                ModelState.AddModelError("", "Email or Password are Incorrect");
                return View(loginVM);
            }

            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError("", "Your Account is blocked");
                return View(loginVM);
            }

            //if (appUser.Baskets != null && appUser.Baskets.Count() > 0)
            //{
            //    List<BasketVM> basketVMs = new List<BasketVM>();

            //    foreach (Basket basket in appUser.Baskets)
            //    {
            //        BasketVM basketVM = new BasketVM
            //        {
            //            Id = (int)basket.ProductId,
            //            Count = basket.Count
            //        };
            //        basketVMs.Add(basketVM);
            //    }

            //    string cookie = JsonConvert.SerializeObject(basketVMs);
            //    Response.Cookies.Append("basket", cookie);

            //}

            return RedirectToAction("Index", "Home");
        }

        //3.Logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        //4.Email Confirmation
        public async Task<IActionResult> EmailConfirm(string id, string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null)
            {
                return NotFound();
            }

            if (!appUser.IsActive)
            {
                return BadRequest();
            }

            if (appUser.EmailConfirmed)
            {
                return Conflict();
            }

            IdentityResult identityResult = await _userManager.ConfirmEmailAsync(appUser, token);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(nameof(Login));
            }

            await _signInManager.SignInAsync(appUser, true);

            return RedirectToAction("Index", "Home");
        }

        //5.Edit Profile
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> AccountSettings()
        {
            AppUser appUser = await _userManager.Users
             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            AccountSettingsVM accountSettingsVM = new AccountSettingsVM
            {
                editProfileVM = new EditProfileVM
                {
                    DisplayName = appUser.DisplayName,
                    FirstName = appUser.FirstName,
                    LastName = appUser.LastName,
                    Location = appUser.Location,
                    AboutMe = appUser.AboutMe,
                    ProfilePicture = appUser.ProfilePicture
                },
                changeEmailVM = new ChangeEmailVM
                {
                    Email = appUser.Email,
                },
                changePhoneNumbreVM = new ChangePhoneNumbreVM
                {
                    Phone = appUser.PhoneNumber,
                },
                changeUserNameVM= new ChangeUserNameVM 
                {
                    UserName = appUser.UserName,
                }

            };

            return View(accountSettingsVM);
        }
        [HttpPost]
        [Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileVM editProfileVM)
        {
            AppUser DbAppUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return PartialView("_EditProfilePartial",editProfileVM);
            }

            if (editProfileVM.ProfilePictureFile != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "images", "pfp", DbAppUser.ProfilePicture);
                if (System.IO.File.Exists(filePath) && DbAppUser.ProfilePicture != "default2.jpg")
                {
                    System.IO.File.Delete(filePath);
                }


                DbAppUser.ProfilePicture = await editProfileVM.ProfilePictureFile.Save(_env.WebRootPath, new string[] { "assets", "images", "pfp" });
            }

            DbAppUser.AboutMe = editProfileVM.AboutMe;
            DbAppUser.Location = editProfileVM.Location;
            DbAppUser.FirstName = editProfileVM.FirstName;
            DbAppUser.LastName = editProfileVM.LastName;
            DbAppUser.DisplayName = editProfileVM.DisplayName;



            IdentityResult identityResult = await _userManager.UpdateAsync(DbAppUser);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);

                }
                return View("_EditProfilePartial", editProfileVM);
            }

            await _signInManager.SignInAsync(DbAppUser, true);

            return RedirectToAction(nameof(AccountSettings));
        }

        ////6.Change Email
        //[HttpPost]
        //[Authorize(Roles = "Member")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ChangeEmail(AppUser appUser, string currentPassword) 
        //{
        //    AppUser DbAppUser = await _userManager.Users
        //    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

        //    ViewBag.DbAppUser = DbAppUser;

        //    if (DbAppUser == null)
        //    {
        //        return NotFound();
        //    }

        //    if (string.IsNullOrWhiteSpace(appUser.Email))
        //    {
        //        ModelState.AddModelError("", "Email Is Required");
        //        return View("EditProfile", DbAppUser);
        //    }

        //    if (string.IsNullOrWhiteSpace(currentPassword))
        //    {
        //        ModelState.AddModelError("", "Password Is Required");
        //        return View("EditProfile", DbAppUser);
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        ModelState.AddModelError("", "Email Format Is Incorrect");
        //        return View("EditProfile", DbAppUser);
        //    }

        //    bool PasswordCheck = await _userManager.CheckPasswordAsync(DbAppUser, currentPassword);

        //    if (!PasswordCheck)
        //    {
        //        ModelState.AddModelError("", "Incorrect Password");
        //        return View("EditProfile", DbAppUser);
        //    }

        //    if (DbAppUser.NormalizedEmail != appUser.Email.Trim().ToUpperInvariant())
        //    {
        //        DbAppUser.Email = appUser.Email;
        //    }


        //    IdentityResult identityResult = await _userManager.UpdateAsync(DbAppUser);

        //    if (!identityResult.Succeeded)
        //    {
        //        foreach (IdentityError identityError in identityResult.Errors)
        //        {
        //            ModelState.AddModelError("", identityError.Description);

        //        }
        //        return View("EditProfile", DbAppUser);
        //    }

        //    await _signInManager.SignInAsync(DbAppUser, true);

        //    return RedirectToAction(nameof(EditProfile));
        //}

        ////6.Change UserName
        //[HttpPost]
        //[Authorize(Roles = "Member")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ChangeUserName(AppUser appUser, string currentPassword)
        //{
        //    AppUser DbAppUser = await _userManager.Users
        //    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

        //    if (DbAppUser == null)
        //    {
        //        return NotFound();
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        return View("EditProfile", DbAppUser);
        //    }

        //    if (string.IsNullOrWhiteSpace(currentPassword))
        //    {
        //        ModelState.AddModelError("", "Password Is Required");
        //        return View("EditProfile", DbAppUser);
        //    }

        //    bool PasswordCheck = await _userManager.CheckPasswordAsync(DbAppUser, currentPassword);

        //    if (!PasswordCheck)
        //    {
        //        ModelState.AddModelError("", "Incorrect Password");
        //        return View("EditProfile", DbAppUser);
        //    }

        //    if (DbAppUser.NormalizedUserName != appUser.UserName.Trim().ToUpperInvariant())
        //    {
        //        DbAppUser.UserName = appUser.UserName;
        //    }


        //    IdentityResult identityResult = await _userManager.UpdateAsync(DbAppUser);

        //    if (!identityResult.Succeeded)
        //    {
        //        foreach (IdentityError identityError in identityResult.Errors)
        //        {
        //            ModelState.AddModelError("", identityError.Description);

        //        }
        //        return View("EditProfile", DbAppUser);
        //    }

        //    await _signInManager.SignInAsync(DbAppUser, true);

        //    return RedirectToAction(nameof(EditProfile));
        //}

        ////7.Change PhoneNumber
        //[HttpPost]
        //[Authorize(Roles = "Member")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ChangePhoneNumber(AppUser appUser, string currentPassword)
        //{
        //    AppUser DbAppUser = await _userManager.Users
        //    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

        //    if (DbAppUser == null)
        //    {
        //        return NotFound();
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        return View("EditProfile", DbAppUser);
        //    }

        //    if (string.IsNullOrWhiteSpace(currentPassword))
        //    {
        //        ModelState.AddModelError("", "Password Is Required");
        //        return View("EditProfile", DbAppUser);
        //    }

        //    bool PasswordCheck = await _userManager.CheckPasswordAsync(DbAppUser, currentPassword);

        //    if (!PasswordCheck)
        //    {
        //        ModelState.AddModelError("", "Incorrect Password");
        //        return View("EditProfile", DbAppUser);
        //    }

        //    if (DbAppUser.PhoneNumber != appUser.PhoneNumber.Trim())
        //    {
        //        DbAppUser.PhoneNumber = appUser.PhoneNumber;
        //    }


        //    IdentityResult identityResult = await _userManager.UpdateAsync(DbAppUser);

        //    if (!identityResult.Succeeded)
        //    {
        //        foreach (IdentityError identityError in identityResult.Errors)
        //        {
        //            ModelState.AddModelError("", identityError.Description);

        //        }
        //        return View("EditProfile", DbAppUser);
        //    }

        //    await _signInManager.SignInAsync(DbAppUser, true);

        //    return RedirectToAction(nameof(EditProfile));
        //}

        ////8.Change Password
        //[HttpPost]
        //[Authorize(Roles = "Member")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ChangePassword(string newPassword, string confirmPassword, string currentPassword)
        //{
        //    AppUser DbAppUser = await _userManager.Users
        //    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

        //    if (DbAppUser == null)
        //    {
        //        return NotFound();
        //    }

        //    if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        //    {
        //        ModelState.AddModelError("", "All Password Inputs Are Required");
        //        return View("EditProfile",DbAppUser);
        //    }

        //    string passwordRegex = @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$";

        //    if (!Regex.IsMatch(newPassword, passwordRegex))
        //    {
        //        ModelState.AddModelError("", "At least one digit ,one lowercase letter, one uppercase letter, one alphabetic character (letter) and minimum length of 8 characters is required");
        //        return View("EditProfile", DbAppUser);
        //    }

        //    if (newPassword != confirmPassword)
        //    {
        //        ModelState.AddModelError("", "New Password and Confirm Password Should Match");
        //        return View("EditProfile", DbAppUser);
        //    }

        //    bool PasswordCheck = await _userManager.CheckPasswordAsync(DbAppUser, currentPassword);

        //    if (!PasswordCheck)
        //    {
        //        ModelState.AddModelError("", "Incorrect Current Password");
        //        return View("EditProfile", DbAppUser);
        //    }



        //    IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(DbAppUser, currentPassword, newPassword);

        //    if (!changePasswordResult.Succeeded)
        //    {
        //        foreach (var error in changePasswordResult.Errors)
        //        {
        //            ModelState.AddModelError("", error.Description);
        //        }
        //        return View("EditProfile", DbAppUser);
        //    }

        //    await _signInManager.SignInAsync(DbAppUser, true);

        //    return RedirectToAction(nameof(EditProfile));
        //}
        #region RoleCreation
        //public async Task<IActionResult> CreateRole()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Admin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Member"));


        //    return Ok("Roles Created");
        //}

        //public async Task<IActionResult> SuperAdmin()
        //{
        //    AppUser appUser = new AppUser
        //    {
        //        Email = "SuperAdmin@mail.com",
        //        UserName = "superadmin"

        //    };

        //    await _userManager.CreateAsync(appUser, "SuperAdmin666");
        //    await _userManager.AddToRoleAsync(appUser, "SuperAdmin");

        //    return Ok("SuperAdmin Created");
        //}

        //public async Task<IActionResult> Admin()
        //{
        //    AppUser appUser = new AppUser
        //    {
        //        Email = "Admin@mail.com",
        //        UserName = "admin"

        //    };

        //    await _userManager.CreateAsync(appUser, "Admin666");
        //    await _userManager.AddToRoleAsync(appUser, "Admin");

        //    return Ok("Admin Created");
        //}
        #endregion
    }
}
