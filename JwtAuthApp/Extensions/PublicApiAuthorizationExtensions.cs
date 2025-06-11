using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace JwtAuthApp.Extensions
{
    public static class PublicApiAuthorizationExtensions
    {
        /// <summary>
        /// Configura políticas de autorização específicas para API pública
        /// </summary>
        public static IServiceCollection AddPublicApiPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Política base para API pública - qualquer token válido de API pública
                options.AddPolicy("PublicApiAccess", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("token_type", "public_api_access");
                    policy.RequireClaim("scope", "public_api");
                });

                // Políticas específicas por permissão
                options.AddPolicy("PublicApi.Read", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("token_type", "public_api_access");
                    policy.Requirements.Add(new PublicApiPermissionRequirement("api.public.read"));
                });

                options.AddPolicy("PublicApi.Create", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("token_type", "public_api_access");
                    policy.Requirements.Add(new PublicApiPermissionRequirement("api.public.create"));
                });

                options.AddPolicy("PublicApi.Update", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("token_type", "public_api_access");
                    policy.Requirements.Add(new PublicApiPermissionRequirement("api.public.update"));
                });

                options.AddPolicy("PublicApi.Delete", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("token_type", "public_api_access");
                    policy.Requirements.Add(new PublicApiPermissionRequirement("api.public.delete"));
                });

                options.AddPolicy("PublicApi.Admin", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("token_type", "public_api_access");
                    policy.Requirements.Add(new PublicApiPermissionRequirement("api.public.admin"));
                });
            });

            // Registrar o handler para validação de permissões
            services.AddScoped<IAuthorizationHandler, PublicApiPermissionHandler>();

            return services;
        }
    }

    /// <summary>
    /// Requirement customizado para validar permissões da API pública
    /// </summary>
    public class PublicApiPermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PublicApiPermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    /// <summary>
    /// Handler para validar permissões específicas da API pública
    /// </summary>
    public class PublicApiPermissionHandler : AuthorizationHandler<PublicApiPermissionRequirement>
    {
        private readonly ILogger<PublicApiPermissionHandler> _logger;

        public PublicApiPermissionHandler(ILogger<PublicApiPermissionHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PublicApiPermissionRequirement requirement)
        {
            try
            {
                // Verificar se é um token de API pública
                var tokenType = context.User.FindFirst("token_type")?.Value;
                if (tokenType != "public_api_access")
                {
                    _logger.LogWarning("Invalid token type for public API: {TokenType}", tokenType);
                    context.Fail();
                    return Task.CompletedTask;
                }

                // Obter todas as permissões do token
                var permissions = context.User.FindAll("permission").Select(c => c.Value).ToList();

                // Verificar se tem a permissão específica OU permissão de admin
                var hasRequiredPermission = permissions.Contains(requirement.Permission);
                var hasAdminPermission = permissions.Contains("api.public.admin");

                if (hasRequiredPermission || hasAdminPermission)
                {
                    _logger.LogDebug("Access granted for permission: {Permission}", requirement.Permission);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning("Access denied for permission: {Permission}. User permissions: {Permissions}",
                        requirement.Permission, string.Join(", ", permissions));
                    context.Fail();
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating public API permission: {Permission}", requirement.Permission);
                context.Fail();
                return Task.CompletedTask;
            }
        }
    }

    /// <summary>
    /// Atributo para aplicar autorização específica da API pública
    /// </summary>
    public class PublicApiAuthorizeAttribute : AuthorizeAttribute
    {
        public PublicApiAuthorizeAttribute(string permission)
        {
            Policy = permission switch
            {
                "read" => "PublicApi.Read",
                "create" => "PublicApi.Create",
                "update" => "PublicApi.Update",
                "delete" => "PublicApi.Delete",
                "admin" => "PublicApi.Admin",
                _ => "PublicApiAccess"
            };
        }
    }
}