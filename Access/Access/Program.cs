using Access.Data;
using Access.Models;
using Access.Services.Email;
using Access.Services.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

app.MapControllers();

app.Run();