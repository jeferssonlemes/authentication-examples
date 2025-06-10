using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using JwtAuthApp.Settings;

namespace JwtAuthApp.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            // Simplificar e usar configurações diretas do appsettings.json
            var secretKey = config["JwtSettings:Secret"] ?? "MinhaChaveSecretaSuperSegura123456789";
            var issuer = config["JwtSettings:Issuer"] ?? "JwtAuthApp";
            var audience = config["JwtSettings:Audience"] ?? "JwtAuthApp-Users";

            var key = Encoding.UTF8.GetBytes(secretKey);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true, // Desabilitar para facilitar testes
                        ValidIssuer = issuer,
                        ValidateAudience = true, // Desabilitar para facilitar testes
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(5) // Mais tolerante
                    };

                    // Configurar eventos para tratamento de erros
                    opts.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                            Console.WriteLine($"🔍 [{context.Request.Method}] {context.Request.Path} - Token received: {(string.IsNullOrEmpty(token) ? "NO TOKEN" : "TOKEN PRESENT")}");
                            if (!string.IsNullOrEmpty(token))
                            {
                                Console.WriteLine($"Token length: {token.Length}");
                                Console.WriteLine($"Token starts with: {token.Substring(0, Math.Min(20, token.Length))}...");
                            }
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"❌ Authentication failed: {context.Exception.Message}");
                            Console.WriteLine($"Exception type: {context.Exception.GetType().Name}");
                            Console.WriteLine($"Request path: {context.Request.Path}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("✅ Token validated successfully");
                            Console.WriteLine($"User: {context.Principal.Identity?.Name}");
                            Console.WriteLine("Claims in token:");
                            foreach (var claim in context.Principal.Claims)
                            {
                                Console.WriteLine($"  {claim.Type}: {claim.Value}");
                            }

                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Console.WriteLine($"OnChallenge: {context.Error} - {context.ErrorDescription}");

                            // Para requisições de API, retornar JSON
                            if (context.Request.Path.StartsWithSegments("/api"))
                            {
                                context.HandleResponse();
                                context.Response.StatusCode = 401;
                                context.Response.ContentType = "application/json";
                                return context.Response.WriteAsync("{\"error\":\"Unauthorized\"}");
                            }

                            // Para requests da UI, deixar o middleware de status codes cuidar do redirecionamento
                            // Apenas definir o status code 401
                            context.Response.StatusCode = 401;
                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            Console.WriteLine("OnForbidden triggered");

                            // Para requisições de API, retornar JSON
                            if (context.Request.Path.StartsWithSegments("/api"))
                            {
                                context.Response.StatusCode = 403;
                                context.Response.ContentType = "application/json";
                                return context.Response.WriteAsync("{\"error\":\"Forbidden\"}");
                            }

                            // Para requests da UI, deixar o middleware de status codes cuidar do redirecionamento
                            // Apenas definir o status code 403
                            context.Response.StatusCode = 403;
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}