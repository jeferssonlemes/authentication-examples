using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace JwtAuthApp.Areas.Internal.Controllers
{
    [Area("Internal")]
    [ApiController]
    [Route("api/internal/[controller]")]
    [Route("api/[controller]")]
    public class RateLimitController : ControllerBase
    {
        private readonly ILogger<RateLimitController> _logger;

        public RateLimitController(ILogger<RateLimitController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Endpoint para testar a política geral de rate limiting
        /// Limite: 100 req/min com janela deslizante
        /// </summary>
        [HttpGet("test-general")]
        [EnableRateLimiting("GeneralPolicy")]
        public IActionResult TestGeneralPolicy()
        {
            _logger.LogInformation("General policy endpoint accessed from {IP}",
                HttpContext.Connection.RemoteIpAddress);

            return Ok(new
            {
                message = "General policy test successful",
                policy = "GeneralPolicy",
                limit = "100 requests per minute",
                type = "Sliding Window",
                timestamp = DateTime.UtcNow,
                ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                requestId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// Endpoint para testar política de autenticação (mais restritiva)
        /// Limite: 5 req/5min com janela fixa
        /// </summary>
        [HttpPost("test-auth")]
        [EnableRateLimiting("AuthPolicy")]
        [AllowAnonymous]
        public IActionResult TestAuthPolicy([FromBody] object data)
        {
            _logger.LogInformation("Auth policy endpoint accessed from {IP}",
                HttpContext.Connection.RemoteIpAddress);

            return Ok(new
            {
                message = "Auth policy test successful",
                policy = "AuthPolicy",
                limit = "5 requests per 5 minutes",
                type = "Fixed Window",
                timestamp = DateTime.UtcNow,
                data = data,
                ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                requestId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// Endpoint para testar política rigorosa (Token Bucket)
        /// Limite: 10 tokens, reabastece 2 tokens a cada 30s
        /// </summary>
        [HttpGet("test-strict")]
        [EnableRateLimiting("StrictPolicy")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult TestStrictPolicy()
        {
            _logger.LogInformation("Strict policy endpoint accessed from {IP} by user {User}",
                HttpContext.Connection.RemoteIpAddress,
                User?.Identity?.Name);

            return Ok(new
            {
                message = "Strict policy test successful",
                policy = "StrictPolicy",
                limit = "10 tokens, replenish 2 every 30s",
                type = "Token Bucket",
                timestamp = DateTime.UtcNow,
                user = User?.Identity?.Name,
                ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                requestId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// Endpoint para testar política de concorrência
        /// Limite: 5 requisições simultâneas por IP
        /// </summary>
        [HttpGet("test-concurrency")]
        [EnableRateLimiting("ConcurrencyPolicy")]
        public async Task<IActionResult> TestConcurrencyPolicy()
        {
            _logger.LogInformation("Concurrency policy endpoint accessed from {IP}",
                HttpContext.Connection.RemoteIpAddress);

            // Simular processamento longo para testar concorrência
            await Task.Delay(2000);

            return Ok(new
            {
                message = "Concurrency policy test successful",
                policy = "ConcurrencyPolicy",
                limit = "5 concurrent requests per IP",
                type = "Concurrency Limiter",
                timestamp = DateTime.UtcNow,
                processingTime = "2 seconds",
                ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                requestId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// Endpoint para múltiplos testes rápidos
        /// Limite: 20 req/min (política de teste)
        /// </summary>
        [HttpGet("test-rapid")]
        [EnableRateLimiting("TestPolicy")]
        public IActionResult TestRapidRequests()
        {
            _logger.LogInformation("Rapid test endpoint accessed from {IP}",
                HttpContext.Connection.RemoteIpAddress);

            return Ok(new
            {
                message = "Rapid test successful",
                policy = "TestPolicy",
                limit = "20 requests per minute",
                type = "Sliding Window (Test)",
                timestamp = DateTime.UtcNow,
                ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                requestId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// Endpoint sem rate limiting específico (usa global limiter)
        /// </summary>
        [HttpGet("test-global")]
        public IActionResult TestGlobalPolicy()
        {
            _logger.LogInformation("Global policy endpoint accessed from {IP}",
                HttpContext.Connection.RemoteIpAddress);

            return Ok(new
            {
                message = "Global policy test successful",
                policy = "GlobalLimiter",
                limit = "200 requests per minute per IP",
                type = "Global Sliding Window",
                timestamp = DateTime.UtcNow,
                ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                requestId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// Endpoint para obter informações sobre as políticas de rate limiting
        /// </summary>
        [HttpGet("policies")]
        [AllowAnonymous]
        public IActionResult GetPolicies()
        {
            var policies = new
            {
                policies = new object[]
                {
                    new
                    {
                        name = "GeneralPolicy",
                        type = "Sliding Window",
                        limit = "100 requests per minute",
                        segments = 6,
                        endpoint = "/api/ratelimit/test-general"
                    },
                    new
                    {
                        name = "AuthPolicy",
                        type = "Fixed Window",
                        limit = "5 requests per 5 minutes",
                        window = "5 minutes",
                        endpoint = "/api/ratelimit/test-auth"
                    },
                    new
                    {
                        name = "StrictPolicy",
                        type = "Token Bucket",
                        limit = "10 tokens, 2 per 30s",
                        replenishment = "30 seconds",
                        endpoint = "/api/ratelimit/test-strict"
                    },
                    new
                    {
                        name = "ConcurrencyPolicy",
                        type = "Concurrency Limiter",
                        limit = "5 concurrent requests",
                        queue = "10 requests",
                        endpoint = "/api/ratelimit/test-concurrency"
                    },
                    new
                    {
                        name = "TestPolicy",
                        type = "Sliding Window",
                        limit = "20 requests per minute",
                        segments = 4,
                        endpoint = "/api/ratelimit/test-rapid"
                    },
                    new
                    {
                        name = "GlobalLimiter",
                        type = "Global Sliding Window",
                        limit = "200 requests per minute per IP",
                        segments = 6,
                        endpoint = "/api/ratelimit/test-global"
                    }
                },
                responseHeaders = new
                {
                    retryAfter = "Retry-After",
                    rateLimitPolicy = "X-RateLimit-Policy",
                    rateLimitRemaining = "X-RateLimit-Remaining"
                },
                statusCodes = new
                {
                    success = 200,
                    rateLimitExceeded = 429
                },
                timestamp = DateTime.UtcNow
            };

            return Ok(policies);
        }

        /// <summary>
        /// Endpoint para simular tentativas de login (usa política de auth)
        /// </summary>
        [HttpPost("simulate-login")]
        [EnableRateLimiting("AuthPolicy")]
        [AllowAnonymous]
        public IActionResult SimulateLogin([FromBody] LoginAttempt attempt)
        {
            _logger.LogInformation("Login simulation from {IP} for user {Username}",
                HttpContext.Connection.RemoteIpAddress, attempt.Username);

            // Simular verificação de credenciais
            var isSuccess = attempt.Username == "test" && attempt.Password == "test";

            return Ok(new
            {
                success = isSuccess,
                message = isSuccess ? "Login successful" : "Invalid credentials",
                policy = "AuthPolicy - 5 attempts per 5 minutes",
                username = attempt.Username,
                timestamp = DateTime.UtcNow,
                ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                requestId = HttpContext.TraceIdentifier
            });
        }
    }

    public class LoginAttempt
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}