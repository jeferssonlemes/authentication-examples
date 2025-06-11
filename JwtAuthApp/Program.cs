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

// Configurar Rate Limiting com políticas usando extensão
builder.Services.AddRateLimitingPolicies();

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

// Aplicar headers de segurança
app.UseSecurityHeaders();

app.UseHttpsRedirection();
app.UseStaticFiles();

// Aplicar Rate Limiting ANTES de autenticação para proteger endpoints de login
app.UseRateLimitingWithHeaders();

app.UseAuthentication();
app.UseAuthorization();

// Configurar rotas MVC (para controllers de Views como Home, Error)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configurar rotas para Areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Configurar rotas da API com Areas
app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
