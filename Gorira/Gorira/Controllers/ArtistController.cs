using Microsoft.AspNetCore.Mvc;

namespace Gorira.Controllers
{
    public class ArtistController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
