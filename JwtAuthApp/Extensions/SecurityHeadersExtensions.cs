namespace JwtAuthApp.Extensions
{
    public static class SecurityHeadersExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                // Verificar se os headers já foram adicionados para evitar duplicação
                // (isso pode acontecer quando UseStatusCodePagesWithReExecute re-executa o pipeline)

                // X-Content-Type-Options: previne MIME type sniffing
                if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
                    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

                // X-Frame-Options: previne clickjacking
                if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
                    context.Response.Headers.Add("X-Frame-Options", "DENY");

                // X-XSS-Protection: ativa proteção XSS do browser
                if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
                    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

                // Content-Security-Policy: política rigorosa de segurança de conteúdo
                if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
                    context.Response.Headers.Add("Content-Security-Policy",
                        "default-src 'self'; " +
                        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
                        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
                        "font-src 'self' https://cdnjs.cloudflare.com; " +
                        "img-src 'self' data:; " +
                        "connect-src 'self'; " +
                        "frame-ancestors 'none';");

                // Referrer-Policy: controla informações de referrer
                if (!context.Response.Headers.ContainsKey("Referrer-Policy"))
                    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

                await next();
            });
        }
    }
}