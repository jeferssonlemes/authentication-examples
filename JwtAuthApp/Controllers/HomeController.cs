using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Se já está logado, redireciona para dashboard
            if (Request.Cookies.ContainsKey("jwt_token"))
            {
                return RedirectToAction("Dashboard");
            }

            return View("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Products()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }

        public IActionResult Admin()
        {
            return View();
        }

        public IActionResult RateLimit()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt_token");
            return RedirectToAction("Login");
        }
    }
}