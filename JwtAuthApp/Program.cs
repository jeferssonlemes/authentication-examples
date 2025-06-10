using JwtAuthApp.Services;
using JwtAuthApp.Extensions;
using JwtAuthApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// Configurar autenticação JWT usando extensão
builder.Services.AddJwtAuthentication(builder.Configuration);

// Configurar autorização com políticas usando extensão
builder.Services.AddPermissionPolicies();

// Registrar serviços
builder.Services.AddScoped<AuthService>();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowAll");

// Middleware de tratamento de status codes
app.UseStatusCodeHandling();

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
