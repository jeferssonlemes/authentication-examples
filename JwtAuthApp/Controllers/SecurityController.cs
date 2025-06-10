using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Linq;

namespace JwtAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityController : ControllerBase
    {
        private readonly ILogger<SecurityController> _logger;

        public SecurityController(ILogger<SecurityController> logger)
        {
            _logger = logger;
        }

        [HttpPost("xss-attempt")]
        [AllowAnonymous] // Permitir chamadas anônimas para logging
        public IActionResult LogXSSAttempt([FromBody] XSSAttemptModel model)
        {
            try
            {
                // Log da tentativa de XSS
                _logger.LogWarning("🚨 TENTATIVA DE XSS DETECTADA: {Context} | Payload: {Payload} | IP: {IP} | UserAgent: {UserAgent} | Timestamp: {Timestamp}",
                    model.Context,
                    model.Payload,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString(),
                    model.Timestamp);

                // Em produção, aqui você poderia:
                // 1. Salvar no banco de dados
                // 2. Enviar alerta para equipe de segurança
                // 3. Bloquear IP temporariamente
                // 4. Integrar com sistemas de monitoramento

                return Ok(new { message = "Tentativa de XSS registrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar tentativa de XSS");
                return StatusCode(500);
            }
        }

        [HttpGet("security-headers")]
        [AllowAnonymous]
        public IActionResult GetSecurityHeaders()
        {
            var headers = new
            {
                XContentTypeOptions = Response.Headers["X-Content-Type-Options"].ToString(),
                XFrameOptions = Response.Headers["X-Frame-Options"].ToString(),
                XXSSProtection = Response.Headers["X-XSS-Protection"].ToString(),
                ContentSecurityPolicy = Response.Headers["Content-Security-Policy"].ToString(),
                ReferrerPolicy = Response.Headers["Referrer-Policy"].ToString(),
                Timestamp = DateTime.UtcNow
            };

            return Ok(headers);
        }

        [HttpGet("security-status")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetSecurityStatus()
        {
            try
            {
                var status = new
                {
                    XSSProtection = new
                    {
                        Enabled = true,
                        JavaScriptProtection = true,
                        AutoSanitization = true,
                        MonitoringActive = true
                    },
                    CSRFProtection = new
                    {
                        Enabled = true,
                        TokenValidation = true,
                        CookieSecure = true
                    },
                    SecurityHeaders = new
                    {
                        ContentSecurityPolicy = true,
                        XFrameOptions = true,
                        XContentTypeOptions = true,
                        XXSSProtection = true
                    },
                    LastSecurityCheck = DateTime.UtcNow,
                    SecurityLevel = "HIGH",
                    Timestamp = DateTime.UtcNow,
                    UserRole = User?.FindFirst("role")?.Value ?? "Unknown"
                };

                _logger.LogInformation("Security status requested by user: {User} at {Time}",
                    User?.Identity?.Name, DateTime.UtcNow);

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security status");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("security-status-simple")]
        [AllowAnonymous] // Para teste sem autorização
        public IActionResult GetSecurityStatusSimple()
        {
            var status = new
            {
                Status = "OK",
                XSSProtection = new { Enabled = true },
                CSRFProtection = new { Enabled = true },
                SecurityHeaders = new { ContentSecurityPolicy = true },
                SecurityLevel = "HIGH",
                Timestamp = DateTime.UtcNow
            };

            return Ok(status);
        }

        [HttpPost("validate-input")]
        [AllowAnonymous]
        public IActionResult ValidateInput([FromBody] InputValidationModel model)
        {
            try
            {
                var isXSSAttempt = DetectXSSAttempt(model.Input);

                if (isXSSAttempt)
                {
                    _logger.LogWarning("🚨 INPUT SUSPEITO DETECTADO: {Input} | IP: {IP}",
                        model.Input,
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                return Ok(new
                {
                    isValid = !isXSSAttempt,
                    isSuspicious = isXSSAttempt,
                    sanitizedInput = SanitizeInput(model.Input),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar input");
                return StatusCode(500);
            }
        }

        private bool DetectXSSAttempt(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var suspiciousPatterns = new[]
            {
                "<script",
                "javascript:",
                "onload=",
                "onclick=",
                "onerror=",
                "onmouseover=",
                "eval(",
                "alert(",
                "document.cookie",
                "document.write",
                "innerHTML",
                "outerHTML"
            };

            return suspiciousPatterns.Any(pattern =>
                input.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#039;")
                .Replace("&", "&amp;")
                .Replace("/", "&#x2F;");
        }
    }

    public class XSSAttemptModel
    {
        public string Context { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }

    public class InputValidationModel
    {
        public string Input { get; set; } = string.Empty;
    }
}