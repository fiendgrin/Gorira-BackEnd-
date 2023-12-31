﻿using Gorira.DataAccessLayer;
using Gorira.Interfaces;
using Gorira.Models;
using Gorira.ViewModels.BasketVMs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Gorira.Services
{
    public class LayoutService : ILayoutService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<AppUser> _userManager;


        public LayoutService(IHttpContextAccessor contextAccessor, AppDbContext context, UserManager<AppUser> userManager)
        {
            _contextAccessor = contextAccessor;
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<BasketVM>> GetBasketsAsync()
        {
            List<BasketVM>? basketVMs = null;


            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated && _contextAccessor.HttpContext.User.IsInRole("Member"))
            {
                AppUser appUser = await _userManager.Users
                    .Include(u => u.Baskets.Where(b => b.IsDeleted == false)).ThenInclude(b => b.Track).ThenInclude(t=>t.User)
                    .FirstOrDefaultAsync(u => u.UserName == _contextAccessor.HttpContext.User.Identity.Name);

                if (appUser.Baskets != null && appUser.Baskets.Count() > 0)
                {
                    basketVMs = new List<BasketVM>();

                    foreach (Basket basket in appUser.Baskets)
                    {

                        BasketVM basketVM = new BasketVM();

                        basketVM.Id = (int)basket.TrackId;
                        basketVM.IsUnlimited = basket.IsUnlimited;
                        basketVM.Image = basket.Track.Cover;
                        basketVM.Price = basket.IsUnlimited ? basket.Track.UnlimitedPrice : basket.Track.Price;
                        basketVM.Title = basket.Track.Title;
                        basketVM.AuthorId = basket.Track.UserId;
                        basketVM.AuthorPfp = basket.Track.User.ProfilePicture;
                        basketVM.AuthorName = basket.Track.User.DisplayName;
                        basketVMs.Add(basketVM);
                    }
                }
                else
                {
                    basketVMs = new List<BasketVM>();
                }

                string cookie = JsonConvert.SerializeObject(basketVMs);
                _contextAccessor.HttpContext.Response.Cookies.Append("basket", cookie);
            }
            else
            {
               
                    basketVMs = new List<BasketVM>();
                    return basketVMs;
              
            }

            return basketVMs;
        }


        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
            return settings;
        }

        public async Task<int> GetMessageCountAsync()
        {
            AppUser? currentUser = await _userManager.Users
             .FirstOrDefaultAsync(u => u.UserName == _contextAccessor.HttpContext.User.Identity.Name);

            IEnumerable<Chat>? chats = null;
            if (currentUser != null)
            {
                chats = await _context.Chats
                    .Include(c=>c.ChatLogs.Where(cl=>cl.Seen == false && cl.IsDeleted == false)).ThenInclude(cl=>cl.Messager)
                    .Where(c=>(c.User1Id ==currentUser.Id || c.User2Id == currentUser.Id) && c.IsDeleted == false).ToListAsync();
            }


            int msgCount = 0;

            if ( chats != null )
            {
                foreach (Chat chat in chats)
                {
                    if (chat.ChatLogs !=null)
                    {
                        msgCount += chat.ChatLogs.Where(cl => cl.Messager.Id != currentUser.Id && cl.Seen == false).Count();
                    }
                }
            }

            return msgCount;
        }
    }
}
