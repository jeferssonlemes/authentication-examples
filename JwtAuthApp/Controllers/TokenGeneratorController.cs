using JwtAuthApp.Models;
using JwtAuthApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApp.Controllers
{
    public class TokenGeneratorController : Controller
    {
        private readonly PublicApiTokenService _tokenService;
        private readonly ILogger<TokenGeneratorController> _logger;

        public TokenGeneratorController(PublicApiTokenService tokenService, ILogger<TokenGeneratorController> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Página principal para geração de tokens
        /// </summary>
        public IActionResult Index()
        {
            var model = new TokenGeneratorViewModel
            {
                AvailablePermissions = PublicApiTokenService.AvailablePermissions,
                Request = new PublicApiTokenRequest
                {
                    ExpirationDays = 30,
                    RateLimit = 1000
                }
            };

            return View(model);
        }

        /// <summary>
        /// Gera um novo token para API pública
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Generate([FromBody] PublicApiTokenRequest request)
        {
            try
            {
                // Validações básicas
                if (string.IsNullOrWhiteSpace(request.ClientId))
                {
                    return Json(new { success = false, message = "Client ID é obrigatório" });
                }

                if (string.IsNullOrWhiteSpace(request.ClientName))
                {
                    return Json(new { success = false, message = "Nome do cliente é obrigatório" });
                }

                if (request.Permissions == null || !request.Permissions.Any())
                {
                    return Json(new { success = false, message = "Pelo menos uma permissão deve ser selecionada" });
                }

                // Gerar o token
                var token = _tokenService.GeneratePublicApiToken(request);

                var response = new PublicApiTokenResponse
                {
                    Token = token,
                    ClientId = request.ClientId,
                    ClientName = request.ClientName,
                    Permissions = request.Permissions,
                    ExpiresAt = DateTime.UtcNow.AddDays(request.ExpirationDays ?? 30),
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Token generated for client {ClientId} by user {User}",
                    request.ClientId, User.Identity?.Name);

                return Json(new
                {
                    success = true,
                    message = "Token gerado com sucesso!",
                    token = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token for client {ClientId}", request.ClientId);

                return Json(new
                {
                    success = false,
                    message = "Erro interno ao gerar token. Tente novamente."
                });
            }
        }

        /// <summary>
        /// Endpoint AJAX para validar um token
        /// </summary>
        [HttpPost]
        public IActionResult ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    return Json(new { success = false, message = "Token é obrigatório" });
                }

                var principal = _tokenService.ValidatePublicApiToken(request.Token);
                if (principal == null)
                {
                    return Json(new { success = false, message = "Token inválido ou expirado" });
                }

                var permissions = _tokenService.ExtractPermissionsFromToken(principal);
                var clientId = principal.FindFirst("client_id")?.Value;
                var clientName = principal.FindFirst("client_name")?.Value;
                var exp = principal.FindFirst("exp")?.Value;

                var expirationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp ?? "0")).DateTime;

                return Json(new
                {
                    success = true,
                    message = "Token válido",
                    clientId = clientId,
                    clientName = clientName,
                    permissions = permissions,
                    expiresAt = expirationDate.ToString("dd/MM/yyyy HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return Json(new { success = false, message = "Erro ao validar token" });
            }
        }
    }
}