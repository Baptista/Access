using Access.Data;
using Access.Models;
using Access.Models.Authentication;
using Access.Services.Email;
using Access.Services.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar o DbContext
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Registrar EmailService e sua configuração
var emailConfig = builder.Configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailService, EmailService>();

// Registrar o UserManagement
builder.Services.AddScoped<IUserManagement, UserManagement>();

// Configurar Identity corretamente
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configurar opções de senha, bloqueio, etc. se necessário
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
})
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders(); // Registrar UserManager, SignInManager e RoleManager

// Registrar a autorização
builder.Services.AddAuthorization();

// Incluir Autenticação (opcionalmente JWT ou Cookies)
builder.Services.AddAuthentication();

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", config =>
    {
        config.PermitLimit = 5;  // Max 5 requests
        config.Window = TimeSpan.FromSeconds(10);  // Within 10 seconds
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 2;  // Requests beyond this limit are rejected immediately
    });
});

// Configure Serilog for file logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/log-.txt",         // Log to file
        rollingInterval: RollingInterval.Day)  // Create a new log file daily
    .CreateLogger();

// Replace the default logger with Serilog
builder.Host.UseSerilog();

// Configurar o pipeline da aplicação
var app = builder.Build();

// Condições para desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapear endpoints da API Identity
app.UseHttpsRedirection();
app.UseAuthentication(); // Usar autenticação antes da autorização
app.UseAuthorization();

// Apply the rate limiter globally
app.UseRateLimiter();
// Or apply it to specific endpoints
app.MapPost("/Login", [EnableRateLimiting("fixed")] (LoginModel model) =>
{
    return Results.Ok("Login successful");
});
app.MapPost("/ForgotPassword", [EnableRateLimiting("fixed")] (LoginModel model) =>
{
    return Results.Ok("Login successful");
});
app.MapPost("/Register", [EnableRateLimiting("fixed")] (LoginModel model) =>
{
    return Results.Ok("Login successful");
});
app.MapPost("/ConfirmEmail", [EnableRateLimiting("fixed")] (LoginModel model) =>
{
    return Results.Ok("Login successful");
});
app.MapPost("/LoginWithOTP", [EnableRateLimiting("fixed")] (LoginModel model) =>
{
    return Results.Ok("Login successful");
});
app.MapPost("/ResetPassword", [EnableRateLimiting("fixed")] (LoginModel model) =>
{
    return Results.Ok("Login successful");
});
app.MapPost("/RefreshToken", [EnableRateLimiting("fixed")] (LoginModel model) =>
{
    return Results.Ok("Login successful");
});

app.MapControllers();

app.Run();