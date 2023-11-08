using Microsoft.AspNetCore.Mvc;

namespace Gorira.Controllers
{
    public class PlaylistController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
