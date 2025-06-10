using Microsoft.AspNetCore.Authorization;
using JwtAuthApp.Requirements;
using System.Security.Claims;

namespace JwtAuthApp.Handlers
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            Console.WriteLine($"=== PermissionHandler Debug ===");
            Console.WriteLine($"Required Permission: {requirement.Permission}");
            Console.WriteLine($"User Identity: {context.User.Identity?.Name}");
            Console.WriteLine($"User IsAuthenticated: {context.User.Identity?.IsAuthenticated}");

            // Listar todas as claims do usuário
            Console.WriteLine("All Claims:");
            foreach (var claim in context.User.Claims)
            {
                Console.WriteLine($"  {claim.Type}: {claim.Value}");
            }

            // Verificar se o usuário tem a permissão específica
            var hasPermission = context.User.HasClaim("permission", requirement.Permission);
            Console.WriteLine($"Has permission '{requirement.Permission}': {hasPermission}");

            if (hasPermission)
            {
                Console.WriteLine("✅ Access granted by permission");
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Verificar se é Admin (tem todas as permissões)
            var isAdmin = context.User.HasClaim(ClaimTypes.Role, "Admin");
            Console.WriteLine($"Has role 'Admin': {isAdmin}");

            if (isAdmin)
            {
                Console.WriteLine("✅ Access granted by Admin role");
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            Console.WriteLine("❌ Access denied");
            Console.WriteLine($"=== End PermissionHandler Debug ===");

            return Task.CompletedTask;
        }
    }
}