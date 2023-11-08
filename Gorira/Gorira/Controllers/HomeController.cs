using Microsoft.AspNetCore.Mvc;

namespace Gorira.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
