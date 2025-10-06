using Access.Data;
using Access.DataAccess;
using Access.Models;
using Access.Models.Authentication;
using Access.Models.Lockout;
using Access.Services.Authentication;
using Access.Services.Background;
using Access.Services.Email;
using Access.Services.SecureLog;
using Access.Services.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Registrar EmailService e sua configuração
var emailConfig = builder.Configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailService, EmailService>();
// Registrar o UserManagement
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

// Configure ADO.NET Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISecurityLogRepository, SecurityLogRepository>();

// Configure Services
builder.Services.AddScoped<IUserManagement, UserManagementAdo>();
builder.Services.AddScoped<ISecurityLogService, SecurityLogServiceAdo>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();




// Registrar a autorização
builder.Services.AddAuthorization();

// Incluir Autenticação (opcionalmente JWT ou Cookies)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
    };
});

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

builder.Services.AddHttpClient();

// Configure Serilog for file logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/log-.txt",         // Log to file
        rollingInterval: RollingInterval.Day)  // Create a new log file daily
    .CreateLogger();

// Replace the default logger with Serilog
builder.Host.UseSerilog();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var lockoutSettings = builder.Configuration.GetSection("LockoutSettings").Get<LockoutSettings>()
    ?? new LockoutSettings();
builder.Services.AddSingleton(lockoutSettings);

// Optional: Add the background cleanup service
builder.Services.AddHostedService<LockoutCleanupService>();

// Configurar o pipeline da aplicação
var app = builder.Build();

// Condições para desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Mapear endpoints da API Identity
//app.UseHttpsRedirection();
//app.UseAuthentication(); // Usar autenticação antes da autorização
//app.UseAuthorization();

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