using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace JwtAuthApp.Extensions
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
        {
            services.AddRateLimiter(rateLimiterOptions =>
            {
                // 1. Política Geral - Sliding Window (janela deslizante)
                rateLimiterOptions.AddPolicy("GeneralPolicy", httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 100, // 100 requisições
                            Window = TimeSpan.FromMinutes(1), // Por minuto
                            SegmentsPerWindow = 6, // Janela dividida em 6 segmentos (10s cada)
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 10 // Fila reduzida para testes mais claros
                        }));

                // 2. Política para APIs de Autenticação - Fixed Window (janela fixa)
                rateLimiterOptions.AddPolicy("AuthPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5, // 5 tentativas de login
                            Window = TimeSpan.FromMinutes(5), // Por 5 minutos
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0 // SEM fila para auth - rejeita imediatamente
                        }));

                // 3. Política Rigorosa para endpoints sensíveis - Token Bucket
                rateLimiterOptions.AddPolicy("StrictPolicy", httpContext =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 10, // 10 tokens máximo
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 2, // Fila muito pequena
                            ReplenishmentPeriod = TimeSpan.FromSeconds(30), // Reabastece a cada 30s
                            TokensPerPeriod = 2, // 2 tokens por período
                            AutoReplenishment = true
                        }));

                // 4. Política por IP - Concurrency Limiter
                rateLimiterOptions.AddPolicy("ConcurrencyPolicy", httpContext =>
                    RateLimitPartition.GetConcurrencyLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new ConcurrencyLimiterOptions
                        {
                            PermitLimit = 5, // 5 requisições simultâneas por IP
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 10
                        }));

                // 5. Política para testes (mais permissiva)
                rateLimiterOptions.AddPolicy("TestPolicy", httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 20, // 20 requisições
                            Window = TimeSpan.FromMinutes(1), // Por minuto
                            SegmentsPerWindow = 4, // Janela dividida em 4 segmentos
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 3 // REDUZIDO: Fila de apenas 3 para demonstrar
                        }));

                // 6. Política para demonstrar rejeição imediata
                rateLimiterOptions.AddPolicy("NoQueuePolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5, // 5 requisições
                            Window = TimeSpan.FromMinutes(1), // Por minuto
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0 // SEM fila - rejeita imediatamente
                        }));

                // Configuração global de fallback
                rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    httpContext => RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 200, // Limite global mais alto
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 50
                        }));

                // Configurar comportamento quando limite é excedido
                rateLimiterOptions.OnRejected = async (context, _) =>
                {
                    var httpContext = context.HttpContext;

                    // Log da tentativa bloqueada com mais detalhes
                    var logger = httpContext.RequestServices.GetService<ILogger<Program>>();
                    logger?.LogWarning("🚫 Rate Limit Exceeded: {IP} - {Path} - {UserAgent} - Reason: {Reason}",
                        httpContext.Connection.RemoteIpAddress,
                        httpContext.Request.Path,
                        httpContext.Request.Headers["User-Agent"].ToString(),
                        context.Reason);

                    // Adicionar headers informativos com mais detalhes
                    httpContext.Response.Headers["Retry-After"] = "60";
                    httpContext.Response.Headers["X-RateLimit-Policy"] = "RateLimited";
                    httpContext.Response.Headers["X-RateLimit-Remaining"] = "0";
                    httpContext.Response.Headers["X-RateLimit-Reason"] = context.Reason.ToString();

                    // Para APIs, retornar JSON com mais informações
                    if (httpContext.Request.Path.StartsWithSegments("/api"))
                    {
                        httpContext.Response.StatusCode = 429; // Too Many Requests
                        httpContext.Response.ContentType = "application/json";

                        var response = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = "Rate limit exceeded",
                            message = "Too many requests. Please try again later.",
                            reason = context.Reason.ToString(),
                            retryAfter = "60", // segundos
                            timestamp = DateTime.UtcNow,
                            requestId = httpContext.TraceIdentifier,
                            tip = context.Reason == RateLimitReasonPhrase.QueueLimitExceeded
                                ? "Queue is full. Try again when current requests complete."
                                : "Rate limit exceeded. Wait before making new requests."
                        });

                        await httpContext.Response.WriteAsync(response);
                    }
                    else
                    {
                        // Para páginas web, retornar mensagem simples
                        httpContext.Response.StatusCode = 429;
                        await httpContext.Response.WriteAsync($"Rate limit exceeded: {context.Reason}. Please try again later.");
                    }
                };
            });

            return services;
        }

        public static IApplicationBuilder UseRateLimitingWithHeaders(this IApplicationBuilder app)
        {
            return app.UseRateLimiter();
        }
    }
}