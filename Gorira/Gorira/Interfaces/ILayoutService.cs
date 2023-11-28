using Gorira.ViewModels.BasketVMs;
using Microsoft.AspNetCore.Mvc;

namespace Gorira.Interfaces
{
    public interface ILayoutService
    {
        Task<Dictionary<string, string>> GetSettingsAsync();
        Task<List<BasketVM>> GetBasketsAsync();
        Task<int> GetMessageCountAsync();
    }
}
