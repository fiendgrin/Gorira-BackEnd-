using Gorira.ViewModels.BasketVMs;

namespace Gorira.Interfaces
{
    public interface ILayoutService
    {
        Task<Dictionary<string, string>> GetSettingsAsync();
        Task<List<BasketVM>> GetBasketsAsync();
    }
}
