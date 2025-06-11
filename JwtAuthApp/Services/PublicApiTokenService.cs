using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthApp.Services
{
    public class PublicApiTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PublicApiTokenService> _logger;

        public PublicApiTokenService(IConfiguration configuration, ILogger<PublicApiTokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Permissões disponíveis para API pública
        /// </summary>
        public static readonly Dictionary<string, string> AvailablePermissions = new()
        {
            { "api.public.read", "Permite leitura (GET)" },
            { "api.public.create", "Permite criação (POST)" },
            { "api.public.update", "Permite atualização (PUT)" },
            { "api.public.delete", "Permite exclusão (DELETE)" },
            { "api.public.admin", "Acesso administrativo completo" }
        };

        /// <summary>
        /// Gera um token JWT para API pública com permissões específicas
        /// </summary>
        public string GeneratePublicApiToken(PublicApiTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Generating public API token for client: {ClientId}", request.ClientId);

                var jwtKey = _configuration["JwtSettings:Secret"] ?? "MinhaChaveSecretaSuperSegura123456789";
                var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? "JwtAuthApp";
                var jwtAudience = _configuration["JwtSettings:Audience"] ?? "JwtAuthApp-Users";

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new("sub", request.ClientId),
                    new("client_id", request.ClientId),
                    new("client_name", request.ClientName),
                    new("scope", "public_api"),
                    new("token_type", "public_api_access"),
                    new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new("jti", Guid.NewGuid().ToString()) // JWT ID único
                };

                // Adicionar permissões como claims
                foreach (var permission in request.Permissions)
                {
                    if (AvailablePermissions.ContainsKey(permission))
                    {
                        claims.Add(new Claim("permission", permission));
                        _logger.LogDebug("Added permission: {Permission}", permission);
                    }
                }

                // Adicionar rate limiting personalizado se especificado
                if (request.RateLimit.HasValue)
                {
                    claims.Add(new Claim("rate_limit", request.RateLimit.Value.ToString()));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(request.ExpirationDays ?? 30), // Default 30 dias
                    Issuer = jwtIssuer,
                    Audience = jwtAudience,
                    SigningCredentials = credentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Public API token generated successfully for client: {ClientId}", request.ClientId);

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating public API token for client: {ClientId}", request.ClientId);
                throw;
            }
        }

        /// <summary>
        /// Valida se um token é válido para API pública
        /// </summary>
        public ClaimsPrincipal? ValidatePublicApiToken(string token)
        {
            try
            {
                var jwtKey = _configuration["JwtSettings:Secret"] ?? "MinhaChaveSecretaSuperSegura123456789";
                var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? "JwtAuthApp";
                var jwtAudience = _configuration["JwtSettings:Audience"] ?? "JwtAuthApp-Users";

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                // Verificar se é um token de API pública
                var tokenType = principal.FindFirst("token_type")?.Value;
                if (tokenType != "public_api_access")
                {
                    _logger.LogWarning("Invalid token type for public API: {TokenType}", tokenType);
                    return null;
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate public API token");
                return null;
            }
        }

        /// <summary>
        /// Extrai permissões de um token validado
        /// </summary>
        public List<string> ExtractPermissionsFromToken(ClaimsPrincipal principal)
        {
            return principal.FindAll("permission").Select(c => c.Value).ToList();
        }
    }

    /// <summary>
    /// Request para geração de token da API pública
    /// </summary>
    public class PublicApiTokenRequest
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public int? ExpirationDays { get; set; } = 30;
        public int? RateLimit { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response da geração de token
    /// </summary>
    public class PublicApiTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}