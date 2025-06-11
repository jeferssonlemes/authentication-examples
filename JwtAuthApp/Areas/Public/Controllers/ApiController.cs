using Microsoft.AspNetCore.Mvc;

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
        /// GET endpoint básico
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Public API GET endpoint called at {Time}", DateTime.Now);

            return Ok(new
            {
                message = "Public API GET endpoint",
                timestamp = DateTime.Now,
                method = "GET"
            });
        }

        /// <summary>
        /// POST endpoint básico
        /// </summary>
        [HttpPost]
        public IActionResult Post([FromBody] object data)
        {
            _logger.LogInformation("Public API POST endpoint called at {Time}", DateTime.Now);

            return Ok(new
            {
                message = "Public API POST endpoint",
                timestamp = DateTime.Now,
                method = "POST",
                receivedData = data
            });
        }

        /// <summary>
        /// PUT endpoint básico
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] object data)
        {
            _logger.LogInformation("Public API PUT endpoint called for ID {Id} at {Time}", id, DateTime.Now);

            return Ok(new
            {
                message = "Public API PUT endpoint",
                timestamp = DateTime.Now,
                method = "PUT",
                id = id,
                receivedData = data
            });
        }

        /// <summary>
        /// DELETE endpoint básico
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation("Public API DELETE endpoint called for ID {Id} at {Time}", id, DateTime.Now);

            return Ok(new
            {
                message = "Public API DELETE endpoint",
                timestamp = DateTime.Now,
                method = "DELETE",
                id = id
            });
        }
    }
}