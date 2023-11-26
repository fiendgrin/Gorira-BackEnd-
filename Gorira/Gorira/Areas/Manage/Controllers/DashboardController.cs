using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gorira.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles ="SuperAdmin, Admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
