using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlamStudio.Areas.Salon.Controllers
{
    [Area("Salon")]
    public class HomeController : Controller
    {
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
