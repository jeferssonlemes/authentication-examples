using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        // Endpoint que apenas ADMIN pode acessar
        [HttpGet("system-config")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetSystemConfig()
        {
            return Ok(new
            {
                message = "Configurações do Sistema - Acesso apenas para Administradores",
                configs = new
                {
                    serverName = "JwtAuthApp-Server",
                    version = "1.0.0",
                    environment = "Production",
                    maxUsers = 1000,
                    backupFrequency = "Daily",
                    maintenanceWindow = "02:00-04:00",
                    securityLevel = "High"
                },
                permissions = new
                {
                    canModifyServerSettings = true,
                    canAccessUserData = true,
                    canManageBackups = true,
                    canViewLogs = true
                }
            });
        }

        // Endpoint que apenas ADMIN pode acessar
        [HttpPost("system-config")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult UpdateSystemConfig([FromBody] object configData)
        {
            return Ok(new
            {
                message = "Configurações do sistema atualizadas com sucesso!",
                updatedBy = User.Identity?.Name,
                timestamp = DateTime.UtcNow
            });
        }

        // Endpoint que apenas ADMIN pode acessar
        [HttpGet("system-logs")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetSystemLogs()
        {
            return Ok(new
            {
                message = "Logs do Sistema - Acesso restrito para Administradores",
                logs = new[]
                {
                    new { timestamp = DateTime.UtcNow.AddHours(-1), level = "INFO", message = "Usuário admin fez login", user = "admin", ip = "", productId = 0, service = "", duration = "" },
                    new { timestamp = DateTime.UtcNow.AddHours(-2), level = "WARN", message = "Tentativa de login inválida", user = "", ip = "192.168.1.100", productId = 0, service = "", duration = "" },
                    new { timestamp = DateTime.UtcNow.AddHours(-3), level = "INFO", message = "Produto criado", user = "moderator", ip = "", productId = 123, service = "", duration = "" },
                    new { timestamp = DateTime.UtcNow.AddHours(-4), level = "ERROR", message = "Falha na conexão com banco", user = "", ip = "", productId = 0, service = "UserService", duration = "" },
                    new { timestamp = DateTime.UtcNow.AddHours(-5), level = "INFO", message = "Backup automático concluído", user = "", ip = "", productId = 0, service = "", duration = "2m 15s" }
                },
                totalEntries = 2547,
                criticalErrors = 3,
                warnings = 15
            });
        }

        // Endpoint que ADMIN E MODERATOR podem acessar
        [HttpGet("reports")]
        [Authorize(Policy = "ModeratorOrAbove")]
        public IActionResult GetReports()
        {
            return Ok(new
            {
                message = "Relatórios Avançados - Acesso para Moderadores e Administradores",
                reports = new
                {
                    userActivity = new
                    {
                        totalLogins = 1234,
                        activeUsers = 89,
                        newUsersThisWeek = 12,
                        mostActiveUser = "moderator"
                    },
                    productStats = new
                    {
                        totalProducts = 156,
                        productsAddedThisMonth = 23,
                        topCategory = "Informática",
                        lowStockAlerts = 8
                    },
                    systemPerformance = new
                    {
                        averageResponseTime = "125ms",
                        uptime = "99.8%",
                        peakConcurrentUsers = 67,
                        errorRate = "0.02%"
                    }
                },
                generatedAt = DateTime.UtcNow,
                accessLevel = User.FindFirst("role")?.Value
            });
        }

        // Endpoint que ADMIN E MODERATOR podem acessar
        [HttpGet("analytics")]
        [Authorize(Policy = "ModeratorOrAbove")]
        public IActionResult GetAnalytics()
        {
            return Ok(new
            {
                message = "Analytics Avançado - Disponível para Moderadores e Administradores",
                analytics = new
                {
                    dailyActiveUsers = new[] { 45, 52, 48, 61, 55, 67, 59 },
                    conversionRate = 3.2,
                    popularFeatures = new[]
                    {
                        new { feature = "Dashboard", usage = 95 },
                        new { feature = "Products", usage = 78 },
                        new { feature = "Users", usage = 34 }
                    },
                    geographicDistribution = new
                    {
                        brazil = 78,
                        usa = 12,
                        europe = 8,
                        other = 2
                    }
                },
                insights = new[]
                {
                    "Pico de atividade às 14h",
                    "Produtos de informática são os mais acessados",
                    "Taxa de retenção de usuários aumentou 15%"
                }
            });
        }

        // Endpoints para testar todos os métodos HTTP - AdminOnly
        [HttpGet("test-methods")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult TestMethodsGet()
        {
            Console.WriteLine($"✅ GET /test-methods hit by user: {User.Identity?.Name}");
            return Ok(new
            {
                method = "GET",
                message = "Teste GET AdminOnly funcionou!",
                user = User.Identity?.Name,
                timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("test-methods")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult TestMethodsPost([FromBody] object data)
        {
            Console.WriteLine($"✅ POST /test-methods hit by user: {User.Identity?.Name}");
            return Ok(new
            {
                method = "POST",
                message = "Teste POST AdminOnly funcionou!",
                user = User.Identity?.Name,
                timestamp = DateTime.UtcNow,
                receivedData = data
            });
        }

        [HttpPut("test-methods")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult TestMethodsPut([FromBody] object data)
        {
            Console.WriteLine($"✅ PUT /test-methods hit by user: {User.Identity?.Name}");
            return Ok(new
            {
                method = "PUT",
                message = "Teste PUT AdminOnly funcionou!",
                user = User.Identity?.Name,
                timestamp = DateTime.UtcNow,
                receivedData = data
            });
        }

        [HttpDelete("test-methods")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult TestMethodsDelete()
        {
            Console.WriteLine($"✅ DELETE /test-methods hit by user: {User.Identity?.Name}");
            return Ok(new
            {
                method = "DELETE",
                message = "Teste DELETE AdminOnly funcionou!",
                user = User.Identity?.Name,
                timestamp = DateTime.UtcNow
            });
        }

        // Endpoints para testar todos os métodos HTTP - ModeratorOrAbove
        [HttpGet("test-methods-mod")]
        [Authorize(Policy = "ModeratorOrAbove")]
        public IActionResult TestMethodsModGet()
        {
            Console.WriteLine($"✅ GET /test-methods-mod hit by user: {User.Identity?.Name}");
            return Ok(new
            {
                method = "GET",
                message = "Teste GET ModeratorOrAbove funcionou!",
                user = User.Identity?.Name,
                timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("test-methods-mod")]
        [Authorize(Policy = "ModeratorOrAbove")]
        public IActionResult TestMethodsModPost([FromBody] object data)
        {
            Console.WriteLine($"✅ POST /test-methods-mod hit by user: {User.Identity?.Name}");
            return Ok(new
            {
                method = "POST",
                message = "Teste POST ModeratorOrAbove funcionou!",
                user = User.Identity?.Name,
                timestamp = DateTime.UtcNow,
                receivedData = data
            });
        }

        [HttpPut("test-methods-mod")]
        [Authorize(Policy = "ModeratorOrAbove")]
        public IActionResult TestMethodsModPut([FromBody] object data)
        {
            Console.WriteLine($"✅ PUT /test-methods-mod hit by user: {User.Identity?.Name}");
            return Ok(new
            {
                method = "PUT",
                message = "Teste PUT ModeratorOrAbove funcionou!",
                user = User.Identity?.Name,
                timestamp = DateTime.UtcNow,
                receivedData = data
            });
        }

        [HttpDelete("test-methods-mod")]
        [Authorize(Policy = "ModeratorOrAbove")]
        public IActionResult TestMethodsModDelete()
        {
            Console.WriteLine($"✅ DELETE /test-methods-mod hit by user: {User.Identity?.Name}");
            return Ok(new
            {
                method = "DELETE",
                message = "Teste DELETE ModeratorOrAbove funcionou!",
                user = User.Identity?.Name,
                timestamp = DateTime.UtcNow
            });
        }

        // Endpoint original de clear-cache mantido
        [HttpDelete("clear-cache")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult ClearSystemCache()
        {
            Console.WriteLine($"🗑️ ClearSystemCache endpoint hit by user: {User.Identity?.Name}");
            Console.WriteLine($"User authenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"User claims count: {User.Claims.Count()}");

            return Ok(new
            {
                message = "Cache do sistema limpo com sucesso!",
                operation = "CLEAR_CACHE",
                executedBy = User.Identity?.Name,
                timestamp = DateTime.UtcNow,
                affectedSystems = new[] { "UserCache", "ProductCache", "SessionCache" }
            });
        }
    }
}