using Microsoft.AspNetCore.Authorization;

namespace JwtAuthApp.Requirements
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }
        
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
} 