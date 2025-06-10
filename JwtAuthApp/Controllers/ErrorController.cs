using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApp.Controllers
{
    public class ErrorController : Controller
    {
        // Action principal que aceita status code como parâmetro (usado pelo middleware nativo)
        [Route("Error/{statusCode:int}")]
        public IActionResult HandleErrorCode(int statusCode)
        {
            // Definir o status code na resposta
            Response.StatusCode = statusCode;

            // Redirecionar para a view específica baseada no status code
            return statusCode switch
            {
                404 => View("NotFound"),
                403 => View("Forbidden"),
                401 => View("Unauthorized"),
                500 => View("InternalServerError"),
                _ => View("Index") // View genérica para outros status codes
            };
        }

        // Manter as rotas específicas para compatibilidade
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