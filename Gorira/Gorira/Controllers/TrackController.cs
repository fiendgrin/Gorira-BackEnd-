using Microsoft.AspNetCore.Mvc;

namespace Gorira.Controllers
{
    public class TrackController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
