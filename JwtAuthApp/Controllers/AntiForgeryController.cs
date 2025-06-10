using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AntiForgeryController : ControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public AntiForgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        // Endpoint para obter token CSRF para autenticação
        [HttpGet("token")]
        public IActionResult GetToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

            // Definir o token no header da resposta
            Response.Headers.Add("X-CSRF-TOKEN", tokens.RequestToken);

            return Ok(new
            {
                token = tokens.RequestToken,
                cookieName = "__RequestVerificationToken",
                headerName = "X-CSRF-TOKEN",
                message = "Token CSRF gerado com sucesso"
            });
        }

        // Endpoint para validar token CSRF (para testes)
        [HttpPost("validate")]
        [ValidateAntiForgeryToken]
        public IActionResult ValidateToken([FromBody] object data)
        {
            return Ok(new
            {
                message = "Token CSRF válido!",
                timestamp = DateTime.UtcNow,
                data = data,
                csrfValidated = true
            });
        }

        // Endpoint para obter token para usuários autenticados
        [HttpGet("auth-token")]
        [Authorize]
        public IActionResult GetAuthenticatedToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

            Response.Headers.Add("X-CSRF-TOKEN", tokens.RequestToken);

            return Ok(new
            {
                token = tokens.RequestToken,
                user = User.Identity?.Name,
                cookieName = "__RequestVerificationToken",
                headerName = "X-CSRF-TOKEN",
                message = "Token CSRF para usuário autenticado gerado com sucesso"
            });
        }
    }
}