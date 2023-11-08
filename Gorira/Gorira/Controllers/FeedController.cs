using Microsoft.AspNetCore.Mvc;

namespace Gorira.Controllers
{
    public class FeedController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
