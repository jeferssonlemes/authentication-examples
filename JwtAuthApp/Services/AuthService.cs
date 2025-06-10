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

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            // Dados mocados de usuários
            _users = new List<User>
            {
                new User { Id = 1, Username = "admin", Password = "admin123", Email = "admin@teste.com", Role = "Admin" },
                new User { Id = 2, Username = "user1", Password = "user123", Email = "user1@teste.com", Role = "User" },
                new User { Id = 3, Username = "user2", Password = "user456", Email = "user2@teste.com", Role = "User" }
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
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
} 