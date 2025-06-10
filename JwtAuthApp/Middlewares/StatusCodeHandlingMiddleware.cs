namespace JwtAuthApp.Middlewares
{
    public class StatusCodeHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public StatusCodeHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // Tratar status codes específicos para requests não API
            if (!context.Request.Path.StartsWithSegments("/api") && !context.Response.HasStarted)
            {
                switch (context.Response.StatusCode)
                {
                    case 404:
                        context.Request.Path = "/Error/NotFound";
                        await _next(context);
                        break;
                    case 403:
                        context.Request.Path = "/Error/Forbidden";
                        await _next(context);
                        break;
                    case 401:
                        context.Request.Path = "/Error/Unauthorized";
                        await _next(context);
                        break;
                }
            }
        }
    }

    // Extensão para facilitar o uso
    public static class StatusCodeHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseStatusCodeHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<StatusCodeHandlingMiddleware>();
        }
    }
} 