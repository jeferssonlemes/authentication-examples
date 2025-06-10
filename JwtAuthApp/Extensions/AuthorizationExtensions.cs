using Microsoft.AspNetCore.Authorization;
using JwtAuthApp.Requirements;
using JwtAuthApp.Handlers;
using System.Security.Claims;

namespace JwtAuthApp.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(opts =>
            {
                // Políticas para Dashboard
                opts.AddPolicy("ViewDashboard", p => p.Requirements.Add(new PermissionRequirement("ViewDashboard")));
                opts.AddPolicy("ManageDashboard", p => p.Requirements.Add(new PermissionRequirement("ManageDashboard")));

                // Políticas para Produtos
                opts.AddPolicy("ViewProducts", p => p.Requirements.Add(new PermissionRequirement("ViewProducts")));
                opts.AddPolicy("EditProducts", p => p.Requirements.Add(new PermissionRequirement("EditProducts")));
                opts.AddPolicy("DeleteProducts", p => p.Requirements.Add(new PermissionRequirement("DeleteProducts")));
                opts.AddPolicy("ManageProducts", p => p.Requirements.Add(new PermissionRequirement("ManageProducts")));

                // Políticas para Usuários
                opts.AddPolicy("ViewUsers", p => p.Requirements.Add(new PermissionRequirement("ViewUsers")));
                opts.AddPolicy("EditUsers", p => p.Requirements.Add(new PermissionRequirement("EditUsers")));
                opts.AddPolicy("DeleteUsers", p => p.Requirements.Add(new PermissionRequirement("DeleteUsers")));
                opts.AddPolicy("ManageUsers", p => p.Requirements.Add(new PermissionRequirement("ManageUsers")));

                // Políticas gerais
                opts.AddPolicy("AdminOnly", p => p.RequireClaim(ClaimTypes.Role, "Admin"));
                opts.AddPolicy("ModeratorOrAbove", p => p.RequireClaim(ClaimTypes.Role, "Admin", "Moderator"));
            });

            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            return services;
        }
    }
}