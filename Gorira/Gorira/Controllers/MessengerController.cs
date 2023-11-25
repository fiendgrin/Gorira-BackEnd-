using Gorira.DataAccessLayer;
using Gorira.Models;
using Gorira.ViewModels.MessengerVMs;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Gorira.Controllers
{
    public class MessengerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public MessengerController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {

            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> Index(int? Id)
        {
            Chat? chat = null;
            if (Id != null)
            {
                chat = await _context.Chats
               .Include(c => c.ChatLogs.Where(cl => cl.IsDeleted == false))
               .FirstOrDefaultAsync(c => c.Id == Id);

                if (chat == null) return NotFound();
            }

            AppUser? currentUser = await _userManager.Users
                 .Include(u => u.Chats.Where(c => c.IsDeleted == false)).ThenInclude(c => c.ChatLogs.Where(cl => cl.IsDeleted == false))
               .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);


            IEnumerable<Chat>? chats = null;
            if (User.Identity.IsAuthenticated)
            {
                chats = await _context.Chats
                .Include(c => c.ChatLogs)
                .Include(c => c.User2)
                .Include(c => c.User1)
                .Where(c => c.User1Id == currentUser.Id || c.User2Id == currentUser.Id).ToListAsync();
            }

            MessengerVM messengerVM = new MessengerVM
            {
                Chats = chats,
                CurrentUser = currentUser,
                ChatId = Id,
            };

            return View(messengerVM);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Member")]
        public async Task<IActionResult> Message(string? Id)
        {
            if (string.IsNullOrWhiteSpace(Id)) return BadRequest();

            AppUser? appUser = await _userManager.Users
                .Include(u => u.Chats.Where(c => c.IsDeleted == false)).ThenInclude(c => c.ChatLogs.Where(cl => cl.IsDeleted == false))
                .FirstOrDefaultAsync(u => u.Id == Id && u.IsActive);

            if (appUser == null) return NotFound();

            AppUser? currentUser = await _userManager.Users
                  .Include(u => u.Chats.Where(c => c.IsDeleted == false)).ThenInclude(c => c.ChatLogs.Where(cl => cl.IsDeleted == false))
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            Chat? chatCheck = await _context.Chats.FirstOrDefaultAsync(c => (c.User1Id == currentUser.Id && c.User2Id == appUser.Id) || (c.User1Id == appUser.Id && c.User2Id == currentUser.Id));

            if (chatCheck==null)
            {
                Chat chat = new Chat
                {
                    User1Id = currentUser.Id,
                    User2Id = appUser.Id,
                    CreatedBy = currentUser.UserName,
                };

                await _context.Chats.AddAsync(chat);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { Id = chat.Id });
            }
            else
            {
                return RedirectToAction("Index", new { Id = chatCheck.Id });
            }

          
        }
    }
}
