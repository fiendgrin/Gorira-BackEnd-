using Gorira.ViewModels.BasketVMs;

namespace Gorira.ViewModels.CartVMs
{
    public class CartVM
    {
        public List<BasketVM>? BasketVMs { get; set; }

        public string UserId { get; set; }
    }
}
