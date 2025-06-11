using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using JwtAuthApp.Models;
using JwtAuthApp.Services;

namespace JwtAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthPolicy")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = _authService.Login(request);

                if (response == null)
                {
                    return Unauthorized(new { message = "Usuário ou senha inválidos" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro interno do servidor", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // Em uma implementação real, aqui você poderia invalidar o token
            // Como estamos usando JWT stateless, apenas retornamos sucesso
            return Ok(new { message = "Logout realizado com sucesso" });
        }

        [HttpGet("profile")]
        [Authorize]
        [EnableRateLimiting("GeneralPolicy")]
        public IActionResult GetProfile()
        {
            var username = User.Identity?.Name;
            var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            return Ok(new
            {
                username = username,
                email = email,
                role = role
            });
        }
    }
}