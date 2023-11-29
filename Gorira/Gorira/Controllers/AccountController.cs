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
using Gorira.ViewModels.BasketVMs;
using Newtonsoft.Json;

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
        //7.Change UserName
        //8.Change PhoneNumber
        //9.Change Password
        //10.Edit Social Media

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

            TempData["Info"] = "Please confirm your email address";

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
                .Include(u => u.Baskets.Where(b => b.IsDeleted == false))
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

            if (appUser.Baskets != null && appUser.Baskets.Count() > 0)
            {
                List<BasketVM> basketVMs = new List<BasketVM>();

                foreach (Basket basket in appUser.Baskets)
                {
                    BasketVM basketVM = new BasketVM
                    {
                        Id = (int)basket.TrackId,
                     IsUnlimited = basket.IsUnlimited,
                    };
                    basketVMs.Add(basketVM);
                }

                string cookie = JsonConvert.SerializeObject(basketVMs);
                Response.Cookies.Append("basket", cookie);

            }

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
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> AccountSettings()
        {
            TempData["Tab"] = "Profile";
            TempData["Credentials"] = "";
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
                changePhoneNumberVM = new ChangePhoneNumberVM
                {
                    Phone = appUser.PhoneNumber,
                },
                changeUserNameVM = new ChangeUserNameVM
                {
                    UserName = appUser.UserName,
                },
                editSocialMediaVM = new EditSocialMediaVM
                {
                    YouTube = appUser.YouTube,
                    Facebook = appUser.Facebook,
                    VK = appUser.VK,
                    Instagram = appUser.Instagram,
                    SoundCloud = appUser.SoundCloud,
                    Twitter = appUser.Twitter
                }

            };

            return View(accountSettingsVM);
        }
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileVM editProfileVM)
        {
            TempData["Tab"] = "Profile";
            TempData["Credentials"] = "";
            AppUser DbAppUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            AccountSettingsVM accountSettingsVM = new AccountSettingsVM
            {
                editProfileVM = new EditProfileVM
                {
                    DisplayName = editProfileVM.DisplayName,
                    FirstName = editProfileVM.FirstName,
                    LastName = editProfileVM.LastName,
                    Location = editProfileVM.Location,
                    AboutMe = editProfileVM.AboutMe,
                    ProfilePicture = editProfileVM.ProfilePicture
                },
                changeEmailVM = new ChangeEmailVM
                {
                    Email = DbAppUser.Email,
                },
                changePhoneNumberVM = new ChangePhoneNumberVM
                {
                    Phone = DbAppUser.PhoneNumber,
                },
                changeUserNameVM = new ChangeUserNameVM
                {
                    UserName = DbAppUser.UserName,
                },
                editSocialMediaVM = new EditSocialMediaVM
                {
                    YouTube = DbAppUser.YouTube,
                    Facebook = DbAppUser.Facebook,
                    VK = DbAppUser.VK,
                    Instagram = DbAppUser.Instagram,
                    SoundCloud = DbAppUser.SoundCloud,
                    Twitter = DbAppUser.Twitter
                }

            };

            if (!ModelState.IsValid)
            {
                TempData["Tab"] = "Profile";
                TempData["Credentials"] = "";
                return View("AccountSettings", accountSettingsVM);
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
                TempData["Tab"] = "Profile";
                TempData["Credentials"] = "";
                return View("AccountSettings", accountSettingsVM);
            }

            await _signInManager.SignInAsync(DbAppUser, true);

            return RedirectToAction(nameof(AccountSettings));
        }

        //6.Change Email
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailVM changeEmailVM)
        {

            AppUser DbAppUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            AccountSettingsVM accountSettingsVM = new AccountSettingsVM
            {
                editProfileVM = new EditProfileVM
                {
                    DisplayName = DbAppUser.DisplayName,
                    FirstName = DbAppUser.FirstName,
                    LastName = DbAppUser.LastName,
                    Location = DbAppUser.Location,
                    AboutMe = DbAppUser.AboutMe,
                    ProfilePicture = DbAppUser.ProfilePicture
                },
                changeEmailVM = new ChangeEmailVM
                {
                    Email = changeEmailVM.Email,
                },
                changePhoneNumberVM = new ChangePhoneNumberVM
                {
                    Phone = DbAppUser.PhoneNumber,
                },
                changeUserNameVM = new ChangeUserNameVM
                {
                    UserName = DbAppUser.UserName,
                },
                editSocialMediaVM = new EditSocialMediaVM
                {
                    YouTube = DbAppUser.YouTube,
                    Facebook = DbAppUser.Facebook,
                    VK = DbAppUser.VK,
                    Instagram = DbAppUser.Instagram,
                    SoundCloud = DbAppUser.SoundCloud,
                    Twitter = DbAppUser.Twitter
                }

            };

            if (!ModelState.IsValid)
            {

                TempData["Tab"] = "Credentials";
                TempData["Credentials"] = "Email";
                return View("AccountSettings", accountSettingsVM);
            }

            bool PasswordCheck = await _userManager.CheckPasswordAsync(DbAppUser, changeEmailVM.CurrentPassword);

            if (!PasswordCheck)
            {

                TempData["Tab"] = "Credentials";
                TempData["Credentials"] = "Email";
                ModelState.AddModelError("", "Incorrect Password");
                return View("AccountSettings", accountSettingsVM);

            }

            if (DbAppUser.NormalizedEmail != changeEmailVM.Email.Trim().ToUpperInvariant())
            {
                DbAppUser.Email = changeEmailVM.Email;
            }


            IdentityResult identityResult = await _userManager.UpdateAsync(DbAppUser);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);

                }

                TempData["Tab"] = "Credentials";
                TempData["Credentials"] = "Email";
                return View("AccountSettings", accountSettingsVM);
            }

            await _signInManager.SignInAsync(DbAppUser, true);

            return RedirectToAction(nameof(AccountSettings));
        }

        //7.Change UserName
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserName(ChangeUserNameVM changeUserNameVM)
        {

            AppUser DbAppUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            AccountSettingsVM accountSettingsVM = new AccountSettingsVM
            {
                editProfileVM = new EditProfileVM
                {
                    DisplayName = DbAppUser.DisplayName,
                    FirstName = DbAppUser.FirstName,
                    LastName = DbAppUser.LastName,
                    Location = DbAppUser.Location,
                    AboutMe = DbAppUser.AboutMe,
                    ProfilePicture = DbAppUser.ProfilePicture
                },
                changeEmailVM = new ChangeEmailVM
                {
                    Email = DbAppUser.Email,
                },
                changePhoneNumberVM = new ChangePhoneNumberVM
                {
                    Phone = DbAppUser.PhoneNumber,
                },
                changeUserNameVM = new ChangeUserNameVM
                {
                    UserName = changeUserNameVM.UserName,
                },
                editSocialMediaVM = new EditSocialMediaVM
                {
                    YouTube = DbAppUser.YouTube,
                    Facebook = DbAppUser.Facebook,
                    VK = DbAppUser.VK,
                    Instagram = DbAppUser.Instagram,
                    SoundCloud = DbAppUser.SoundCloud,
                    Twitter = DbAppUser.Twitter
                }

            };


            if (!ModelState.IsValid)
            {
                TempData["Tab"] = "Credentials";
                TempData["Credentials"] = "Name";
                return View("AccountSettings", accountSettingsVM);
            }

            bool PasswordCheck = await _userManager.CheckPasswordAsync(DbAppUser, changeUserNameVM.CurrentPassword);

            if (!PasswordCheck)
            {

                TempData["Tab"] = "Credentials";
                TempData["Credentials"] = "Name";
                ModelState.AddModelError("", "Incorrect Password");
                return View("AccountSettings", accountSettingsVM);

            }

            if (DbAppUser.NormalizedUserName != changeUserNameVM.UserName.Trim().ToUpperInvariant())
            {
                DbAppUser.UserName = changeUserNameVM.UserName;
            }


            IdentityResult identityResult = await _userManager.UpdateAsync(DbAppUser);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);

                }
                TempData["Tab"] = "Credentials";
                TempData["Credentials"] = "Name";
                return View("AccountSettings", accountSettingsVM);
            }

            await _signInManager.SignInAsync(DbAppUser, true);

            return RedirectToAction(nameof(AccountSettings));
        }

        //8.Change PhoneNumber
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePhoneNumber(ChangePhoneNumberVM changePhoneNumberVM)
        {

            AppUser DbAppUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            AccountSettingsVM accountSettingsVM = new AccountSettingsVM
            {
                editProfileVM = new EditProfileVM
                {
                    DisplayName = DbAppUser.DisplayName,
                    FirstName = DbAppUser.FirstName,
                    LastName = DbAppUser.LastName,
                    Location = DbAppUser.Location,
                    AboutMe = DbAppUser.AboutMe,
                    ProfilePicture = DbAppUser.ProfilePicture
                },
                changeEmailVM = new ChangeEmailVM
                {
                    Email = DbAppUser.Email,
                },
                changePhoneNumberVM = new ChangePhoneNumberVM
                {
                    Phone = changePhoneNumberVM.Phone,
                },
                changeUserNameVM = new ChangeUserNameVM
                {
                    UserName = DbAppUser.UserName,
                },
                editSocialMediaVM = new EditSocialMediaVM
                {
                    YouTube = DbAppUser.YouTube,
                    Facebook = DbAppUser.Facebook,
                    VK = DbAppUser.VK,
                    Instagram = DbAppUser.Instagram,
                    SoundCloud = DbAppUser.SoundCloud,
                    Twitter = DbAppUser.Twitter
                }

            };

            if (!ModelState.IsValid)
            {
                TempData["Tab"] = "Credentials";
                TempData["Credentials"] = "Phone";
                return View("AccountSettings", accountSettingsVM);
            }

            bool PasswordCheck = await _userManager.CheckPasswordAsync(DbAppUser, changePhoneNumberVM.CurrentPassword);

            if (!PasswordCheck)
            {

                TempData["Tab"] = "Credentials";
                TempData["Credentials"] = "Phone";
                ModelState.AddModelError("", "Incorrect Password");
                return View("AccountSettings", accountSettingsVM);

            }

            if (DbAppUser.PhoneNumber != changePhoneNumberVM.Phone.Trim())
            {
                DbAppUser.PhoneNumber = changePhoneNumberVM.Phone;
            }


            IdentityResult identityResult = await _userManager.UpdateAsync(DbAppUser);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);

                }
                TempData["Tab"] = "Credentials";
                TempData["Credentials"] = "Phone";
                return View("AccountSettings", accountSettingsVM);
            }

            await _signInManager.SignInAsync(DbAppUser, true);

            return RedirectToAction(nameof(AccountSettings));
        }

        //9.Change Password
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM changePasswordVM)
        {
            AppUser DbAppUser = await _userManager.Users
             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            AccountSettingsVM accountSettingsVM = new AccountSettingsVM
            {
                editProfileVM = new EditProfileVM
                {
                    DisplayName = DbAppUser.DisplayName,
                    FirstName = DbAppUser.FirstName,
                    LastName = DbAppUser.LastName,
                    Location = DbAppUser.Location,
                    AboutMe = DbAppUser.AboutMe,
                    ProfilePicture = DbAppUser.ProfilePicture
                },
                changeEmailVM = new ChangeEmailVM
                {
                    Email = DbAppUser.Email,
                },
                changePhoneNumberVM = new ChangePhoneNumberVM
                {
                    Phone = DbAppUser.PhoneNumber,
                },
                changeUserNameVM = new ChangeUserNameVM
                {
                    UserName = DbAppUser.UserName,
                },
                editSocialMediaVM = new EditSocialMediaVM
                {
                    YouTube = DbAppUser.YouTube,
                    Facebook = DbAppUser.Facebook,
                    VK = DbAppUser.VK,
                    Instagram = DbAppUser.Instagram,
                    SoundCloud = DbAppUser.SoundCloud,
                    Twitter = DbAppUser.Twitter
                }

            };

            if (!ModelState.IsValid)
            {
                TempData["Tab"] = "Credentials";
                TempData["Credentials"] = "Password";
                return View("AccountSettings", accountSettingsVM);
            }


            bool PasswordCheck = await _userManager.CheckPasswordAsync(DbAppUser, changePasswordVM.CurrentPassword);

            if (!PasswordCheck)
            {

                TempData["Tab"] = "Credentials";
                TempData["Credentials"] = "Password";
                ModelState.AddModelError("", "Incorrect Password");
                return View("AccountSettings", accountSettingsVM);

            }

            IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(DbAppUser, changePasswordVM.CurrentPassword, changePasswordVM.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                TempData["Tab"] = "Credentials";
                TempData["Credentials"] = "Password";
                return View("AccountSettings", accountSettingsVM);
            }

            await _signInManager.SignInAsync(DbAppUser, true);

            return RedirectToAction(nameof(AccountSettings));
        }

        //10.Edit Social Media
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSocialMedia(EditSocialMediaVM editSocialMediaVM)
        {
            TempData["Tab"] = "Social Media";
            TempData["Credentials"] = "";
            AppUser DbAppUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            AccountSettingsVM accountSettingsVM = new AccountSettingsVM
            {
                editProfileVM = new EditProfileVM
                {
                    DisplayName = DbAppUser.DisplayName,
                    FirstName = DbAppUser.FirstName,
                    LastName = DbAppUser.LastName,
                    Location = DbAppUser.Location,
                    AboutMe = DbAppUser.AboutMe,
                    ProfilePicture = DbAppUser.ProfilePicture
                },
                changeEmailVM = new ChangeEmailVM
                {
                    Email = DbAppUser.Email,
                },
                changePhoneNumberVM = new ChangePhoneNumberVM
                {
                    Phone = DbAppUser.PhoneNumber,
                },
                changeUserNameVM = new ChangeUserNameVM
                {
                    UserName = DbAppUser.UserName,
                },
                editSocialMediaVM = new EditSocialMediaVM
                {
                    YouTube = editSocialMediaVM.YouTube,
                    Facebook = editSocialMediaVM.Facebook,
                    VK = editSocialMediaVM.VK,
                    Instagram = editSocialMediaVM.Instagram,
                    SoundCloud = editSocialMediaVM.SoundCloud,
                    Twitter = editSocialMediaVM.Twitter
                }

            };

            if (!ModelState.IsValid)
            {
                TempData["Tab"] = "Social Media";
                TempData["Credentials"] = "";
                return View("AccountSettings", accountSettingsVM);
            }


            string youtubeRegex = @"^(https?:\/\/)?(www\.)?youtube\.com\/(channel|user)\/[\w-]+$";
            string youtubeNewRegex = @"^(https?:\/\/)?(www\.)?youtube\.com\/(?:c\/|channel\/|user\/|@)?[\w-]+\/?$";
            string instagramRegex = @"^(https?:\/\/)?(www\.)?instagram\.com\/[a-zA-Z0-9_\.]+\/?$";
            string twitterRegex = @"^(https?:\/\/)?(www\.)?twitter\.com\/[a-zA-Z0-9_]+\/?$";
            string soundCloudRegex = @"^(https?:\/\/)?(www\.)?soundcloud\.com\/[a-zA-Z0-9_-]+\/?$";
            string facebookRegex = @"^(https?:\/\/)?(www\.)?facebook\.com\/[a-zA-Z0-9\.]+\/?$";
            string vkRegex = @"^(https?:\/\/)?(www\.)?vk\.com\/[a-zA-Z0-9_.]+\/?$";

            if (!string.IsNullOrWhiteSpace(editSocialMediaVM.YouTube)) 
            {
                if (!Regex.IsMatch(editSocialMediaVM.YouTube, youtubeRegex) && !Regex.IsMatch(editSocialMediaVM.YouTube, youtubeNewRegex))
                {
                    TempData["Tab"] = "Social Media";
                    TempData["Credentials"] = "";

                    ModelState.AddModelError("YouTube", "Invalid YouTube channel link format");

                    return View("AccountSettings", accountSettingsVM);
                }
            }

            if (!string.IsNullOrWhiteSpace(editSocialMediaVM.Instagram))
            {
                if (!Regex.IsMatch(editSocialMediaVM.Instagram, instagramRegex))
                {
                    TempData["Tab"] = "Social Media";
                    TempData["Credentials"] = "";

                    ModelState.AddModelError("Instagram", "Invalid Instagram profile link format");

                    return View("AccountSettings", accountSettingsVM);
                }
            }

            if (!string.IsNullOrWhiteSpace(editSocialMediaVM.Twitter))
            {
                if (!Regex.IsMatch(editSocialMediaVM.Twitter, twitterRegex))
                {
                    TempData["Tab"] = "Social Media";
                    TempData["Credentials"] = "";

                    ModelState.AddModelError("Twitter", "Invalid X(Twitter) profile link format");

                    return View("AccountSettings", accountSettingsVM);
                }
            }

            if (!string.IsNullOrWhiteSpace(editSocialMediaVM.SoundCloud))
            {
                if (!Regex.IsMatch(editSocialMediaVM.SoundCloud, soundCloudRegex))
                {
                    TempData["Tab"] = "Social Media";
                    TempData["Credentials"] = "";

                    ModelState.AddModelError("SoundCloud", "Invalid SoundCloud profile link format");

                    return View("AccountSettings", accountSettingsVM);
                }
            }

            if (!string.IsNullOrWhiteSpace(editSocialMediaVM.Facebook))
            {
                if (!Regex.IsMatch(editSocialMediaVM.Facebook, facebookRegex))
                {
                    TempData["Tab"] = "Social Media";
                    TempData["Credentials"] = "";

                    ModelState.AddModelError("Facebook", "Invalid Facebook Profile link format");

                    return View("AccountSettings", accountSettingsVM);
                }
            }

            if (!string.IsNullOrWhiteSpace(editSocialMediaVM.VK))
            {
                if (!Regex.IsMatch(editSocialMediaVM.VK, vkRegex))
                {
                    TempData["Tab"] = "Social Media";
                    TempData["Credentials"] = "";

                    ModelState.AddModelError("VK", "Invalid VK Profile link format");

                    return View("AccountSettings", accountSettingsVM);
                }
            }

            DbAppUser.YouTube = editSocialMediaVM.YouTube;
            DbAppUser.Facebook = editSocialMediaVM.Facebook;
            DbAppUser.VK = editSocialMediaVM.VK;
            DbAppUser.Instagram = editSocialMediaVM.Instagram;
            DbAppUser.SoundCloud = editSocialMediaVM.SoundCloud;
            DbAppUser.Twitter = editSocialMediaVM.Twitter;



            IdentityResult identityResult = await _userManager.UpdateAsync(DbAppUser);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);

                }
                TempData["Tab"] = "Social Media";
                TempData["Credentials"] = "";
                return View("AccountSettings", accountSettingsVM);
            }

            await _signInManager.SignInAsync(DbAppUser, true);

            return RedirectToAction(nameof(AccountSettings));
        }

        //11.ForgotPassword(Get)
        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }
        //12.ForgotPassword(Post)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM forgotPasswordVM)
        {
            if (!ModelState.IsValid)
            {
                return View(forgotPasswordVM);
            }

            AppUser appUser = await _userManager.FindByEmailAsync(forgotPasswordVM.Email);
            if (appUser == null)
            {
                ModelState.AddModelError("Email", "Email is Incorrect");
                return View(forgotPasswordVM);
            }


            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            string templateFullPath = Path.Combine(_env.WebRootPath, "templates", "ResetPassword.html");
            string templateContent = await System.IO.File.ReadAllTextAsync(templateFullPath);
            string url = Url.Action("ResetPassword", "Account", new { appUser.Id, token }, Request.Scheme, Request.Host.ToString());

            templateContent = templateContent.Replace("{{action_url}}", url);
            templateContent = templateContent.Replace("{{name}}", appUser.UserName);

            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(_smtpSetting.Email));
            mimeMessage.To.Add(MailboxAddress.Parse(appUser.Email));
            mimeMessage.Subject = "Reset  Password";
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

            TempData["Info"] = "Please check your email inbox";

            return RedirectToAction(nameof(Login));
        }

        //13.ResetPassword(Get)
        public async Task<IActionResult> ResetPassword()
        {
            return View();
        }

        //14.ResetPassword(Post)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string token, ResetPasswordVM resetPasswordVM)
        {
            if (!ModelState.IsValid) return View(resetPasswordVM);

            if (string.IsNullOrWhiteSpace(id))
            {
                ModelState.AddModelError("", "Id Is Incorrect");
                return View(resetPasswordVM);
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                ModelState.AddModelError("", "Token Is Incorrect");
                return View(resetPasswordVM);
            }

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null)
            {
                ModelState.AddModelError("", "Id Is Incorrect");
                return View(resetPasswordVM);
            }

            IdentityResult identityResult = await _userManager.ResetPasswordAsync(appUser, token, resetPasswordVM.Password);

            if (identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }
                return View(resetPasswordVM);
            }

            return View();
        }





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
