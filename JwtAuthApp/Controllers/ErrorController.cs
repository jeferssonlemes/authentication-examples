using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApp.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/NotFound")]
        public new IActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        [Route("Error/Forbidden")]
        public IActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View();
        }

        [Route("Error/Unauthorized")]
        public new IActionResult Unauthorized()
        {
            Response.StatusCode = 401;
            return View();
        }

        [Route("Error/InternalServerError")]
        public IActionResult InternalServerError()
        {
            Response.StatusCode = 500;
            return View();
        }

        [Route("Error")]
        public IActionResult Index()
        {
            return View();
        }
    }
} 