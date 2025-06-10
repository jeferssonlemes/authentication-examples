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
            var jwtSection = config.GetSection("JwtSettings");
            var jwt = jwtSection.Get<JwtSettings>() ?? new JwtSettings();
            
            // Fallback para configurações antigas se não existir a nova seção
            if (string.IsNullOrEmpty(jwt.SecretKey))
            {
                jwt.SecretKey = config["JwtSettings:Secret"] ?? "MinhaChaveSecretaSuperSegura123456789";
                jwt.Issuer = config["JwtSettings:Issuer"] ?? "JwtAuthApp";
                jwt.Audience = config["JwtSettings:Audience"] ?? "JwtAuthApp-Users";
                jwt.ExpirationHours = int.Parse(config["JwtSettings:ExpirationHours"] ?? "1");
            }

            var key = Encoding.UTF8.GetBytes(jwt.SecretKey);

            services.AddSingleton(jwt);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };

                    // Configurar eventos para tratamento de erros
                    opts.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            if (!context.Response.HasStarted)
                            {
                                context.Response.Redirect("/Error/Unauthorized");
                            }
                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            if (!context.Response.HasStarted)
                            {
                                context.Response.Redirect("/Error/Forbidden");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
} 