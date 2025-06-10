using JwtAuthApp.Services;
using JwtAuthApp.Extensions;
using JwtAuthApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// Configurar AntiForgery com configurações de segurança robustas
builder.Services.AddAntiforgery(options =>
{
    // Nome do cookie do token
    options.Cookie.Name = "__RequestVerificationToken";

    // Configurações de segurança do cookie
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS em produção
    options.Cookie.SameSite = SameSiteMode.Strict;

    // Nome do header para APIs
    options.HeaderName = "X-CSRF-TOKEN";

    // Configurar para funcionar com SPA
    options.Cookie.Path = "/";

    // Suprimir para APIs específicas se necessário
    options.SuppressXFrameOptionsHeader = false;
});

// Configurar autenticação JWT usando extensão
builder.Services.AddJwtAuthentication(builder.Configuration);

// Configurar autorização com políticas usando extensão
builder.Services.AddPermissionPolicies();

// Registrar serviços
builder.Services.AddScoped<AuthService>();

// Configurar CORS - ajustar para permitir headers do AntiForgery
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("X-CSRF-TOKEN"); // Expor header do CSRF
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Usar middleware customizado de tratamento de exceções
    app.UseGlobalExceptionHandler();
    app.UseHsts();
}

// Configurar tratamento de status codes usando middleware nativo do ASP.NET Core
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseCors("AllowAll");

// Adicionar headers de segurança para prevenir XSS
app.Use(async (context, next) =>
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Configurar rotas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Manter rotas da API
app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
