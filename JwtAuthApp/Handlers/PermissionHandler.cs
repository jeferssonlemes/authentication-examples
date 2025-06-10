using Microsoft.AspNetCore.Authorization;
using JwtAuthApp.Requirements;

namespace JwtAuthApp.Handlers
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Verificar se o usuário tem a permissão específica
            if (context.User.HasClaim("permission", requirement.Permission))
            {
                context.Succeed(requirement);
            }
            
            // Verificar se é Admin (tem todas as permissões)
            if (context.User.HasClaim("role", "Admin"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
} 