using System.Text.Json;

namespace JwtAuthApp.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado ocorreu.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Se já foi iniciada a resposta, não podemos modificar
            if (context.Response.HasStarted)
                return;

            // Para requisições de API (Content-Type: application/json)
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                
                var result = JsonSerializer.Serialize(new 
                { 
                    error = "Erro interno do servidor",
                    message = exception.Message,
                    timestamp = DateTime.UtcNow
                });
                
                await context.Response.WriteAsync(result);
            }
            else
            {
                // Para requisições web, redirecionar para página de erro
                context.Response.Redirect("/Error/InternalServerError");
            }
        }
    }

    // Extensão para facilitar o uso
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
} 