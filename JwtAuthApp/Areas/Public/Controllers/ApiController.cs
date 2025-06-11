using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JwtAuthApp.Extensions;
using System.Diagnostics;

namespace JwtAuthApp.Areas.Public.Controllers
{
    [Area("Public")]
    [ApiController]
    [Route("api/public/[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;

        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// GET endpoint básico - Requer permissão de leitura
        /// </summary>
        [HttpGet]
        [PublicApiAuthorize("read")]
        public IActionResult Get()
        {
            var clientId = User.FindFirst("client_id")?.Value;
            var clientName = User.FindFirst("client_name")?.Value;

            _logger.LogInformation("Public API GET endpoint called by client {ClientId} at {Time}", clientId, DateTime.Now);

            return Ok(new
            {
                message = "Public API GET endpoint",
                timestamp = DateTime.Now,
                method = "GET",
                client = new
                {
                    id = clientId,
                    name = clientName
                },
                permissions = User.FindAll("permission").Select(c => c.Value).ToList()
            });
        }

        /// <summary>
        /// POST endpoint básico - Requer permissão de criação
        /// </summary>
        [HttpPost]
        [PublicApiAuthorize("create")]
        public IActionResult Post([FromBody] object data)
        {
            var clientId = User.FindFirst("client_id")?.Value;
            var clientName = User.FindFirst("client_name")?.Value;

            _logger.LogInformation("Public API POST endpoint called by client {ClientId} at {Time}", clientId, DateTime.Now);

            return Ok(new
            {
                message = "Public API POST endpoint",
                timestamp = DateTime.Now,
                method = "POST",
                receivedData = data,
                client = new
                {
                    id = clientId,
                    name = clientName
                },
                permissions = User.FindAll("permission").Select(c => c.Value).ToList()
            });
        }

        /// <summary>
        /// PUT endpoint básico - Requer permissão de atualização
        /// </summary>
        [HttpPut("{id}")]
        [PublicApiAuthorize("update")]
        public IActionResult Put(int id, [FromBody] object data)
        {
            var clientId = User.FindFirst("client_id")?.Value;
            var clientName = User.FindFirst("client_name")?.Value;

            _logger.LogInformation("Public API PUT endpoint called by client {ClientId} for ID {Id} at {Time}", clientId, id, DateTime.Now);

            return Ok(new
            {
                message = "Public API PUT endpoint",
                timestamp = DateTime.Now,
                method = "PUT",
                id = id,
                receivedData = data,
                client = new
                {
                    id = clientId,
                    name = clientName
                },
                permissions = User.FindAll("permission").Select(c => c.Value).ToList()
            });
        }

        /// <summary>
        /// DELETE endpoint básico - Requer permissão de exclusão
        /// </summary>
        [HttpDelete("{id}")]
        [PublicApiAuthorize("delete")]
        public IActionResult Delete(int id)
        {
            var clientId = User.FindFirst("client_id")?.Value;
            var clientName = User.FindFirst("client_name")?.Value;

            _logger.LogInformation("Public API DELETE endpoint called by client {ClientId} for ID {Id} at {Time}", clientId, id, DateTime.Now);

            return Ok(new
            {
                message = "Public API DELETE endpoint",
                timestamp = DateTime.Now,
                method = "DELETE",
                id = id,
                client = new
                {
                    id = clientId,
                    name = clientName
                },
                permissions = User.FindAll("permission").Select(c => c.Value).ToList()
            });
        }

        /// <summary>
        /// Endpoint de informações - Apenas autenticação básica
        /// </summary>
        [HttpGet("info")]
        [Authorize(Policy = "PublicApiAccess")]
        public IActionResult GetInfo()
        {
            var clientId = User.FindFirst("client_id")?.Value;
            var clientName = User.FindFirst("client_name")?.Value;
            var permissions = User.FindAll("permission").Select(c => c.Value).ToList();

            return Ok(new
            {
                message = "Public API Info endpoint",
                timestamp = DateTime.Now,
                client = new
                {
                    id = clientId,
                    name = clientName
                },
                permissions = permissions,
                tokenInfo = new
                {
                    issuer = User.FindFirst("iss")?.Value,
                    audience = User.FindFirst("aud")?.Value,
                    subject = User.FindFirst("sub")?.Value,
                    scope = User.FindFirst("scope")?.Value,
                    tokenType = User.FindFirst("token_type")?.Value
                }
            });
        }

        /// <summary>
        /// Endpoint administrativo - Requer permissão de admin
        /// </summary>
        [HttpGet("admin")]
        [PublicApiAuthorize("admin")]
        public IActionResult GetAdminInfo()
        {
            var clientId = User.FindFirst("client_id")?.Value;

            _logger.LogInformation("Public API Admin endpoint accessed by client {ClientId}", clientId);

            return Ok(new
            {
                message = "Public API Admin endpoint",
                timestamp = DateTime.Now,
                adminInfo = new
                {
                    serverTime = DateTime.Now,
                    serverVersion = "1.0.0",
                    environment = Environment.MachineName,
                    totalMemory = GC.GetTotalMemory(false),
                    uptime = DateTime.Now - Process.GetCurrentProcess().StartTime
                },
                client = new
                {
                    id = clientId,
                    name = User.FindFirst("client_name")?.Value
                }
            });
        }
    }
}