using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtAuthApp.Models;

namespace JwtAuthApp.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly List<User> _users;
        private readonly Dictionary<string, List<string>> _userPermissions;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            // Dados mocados de usuários com 3 níveis diferentes
            _users = new List<User>
            {
                new User { Id = 1, Username = "admin", Password = "admin123", Email = "admin@teste.com", Role = "Admin" },
                new User { Id = 2, Username = "moderator", Password = "mod123", Email = "moderator@teste.com", Role = "Moderator" },
                new User { Id = 3, Username = "user", Password = "user123", Email = "user@teste.com", Role = "User" }
            };

            // Permissões específicas por usuário/role
            _userPermissions = new Dictionary<string, List<string>>
            {
                ["Admin"] = new List<string>
                {
                    // Dashboard
                    "ViewDashboard", "ManageDashboard",
                    // Produtos - Admin tem TODAS as permissões
                    "ViewProducts", "EditProducts", "DeleteProducts", "ManageProducts",
                    // Usuários - Admin pode gerenciar usuários
                    "ViewUsers", "EditUsers", "DeleteUsers", "ManageUsers"
                },
                ["Moderator"] = new List<string>
                {
                    // Dashboard
                    "ViewDashboard",
                    // Produtos - Moderador pode ver e editar
                    "ViewProducts", "EditProducts",
                    // Usuários - Moderador pode ver usuários
                    "ViewUsers"
                },
                ["User"] = new List<string>
                {
                    // Dashboard
                    "ViewDashboard",
                    // Produtos - Usuário comum só pode ver
                    "ViewProducts"
                    // Usuários - Usuário comum NÃO pode ver outros usuários
                }
            };
        }

        public LoginResponse? Login(LoginRequest request)
        {
            var user = _users.FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);
            
            if (user == null)
                return null;

            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(1);

            return new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                ExpiresAt = expiresAt
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? "MinhaChaveSecretaSuperSegura123456789");
            
            // Obter permissões do usuário
            var userPermissions = _userPermissions.GetValueOrDefault(user.Role, new List<string>());
            
            // Criar claims básicas
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString()),
                new Claim("role", user.Role) // Claim adicional para facilitar verificação
            };

            // Adicionar permissões como claims
            foreach (var permission in userPermissions)
            {
                claims.Add(new Claim("permission", permission));
            }
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
} 