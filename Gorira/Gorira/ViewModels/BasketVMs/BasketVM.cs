using Gorira.Models;

namespace Gorira.ViewModels.BasketVMs
{
    public class BasketVM
    {

        public int Id { get; set; }
        public bool IsUnlimited { get; set; }
        public string Title { get; set; }
        public double? Price { get; set; }
        public string Image { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorPfp { get; set; }


    }
}
