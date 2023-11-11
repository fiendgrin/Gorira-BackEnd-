using Gorira.DataAccessLayer;
using Gorira.Interfaces;
using Gorira.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
            return settings;
        }
    }
}
