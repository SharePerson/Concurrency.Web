using Microsoft.AspNetCore.Mvc;

namespace Concurrency.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
