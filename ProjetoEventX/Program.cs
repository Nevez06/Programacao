using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// ================================
// 🔹 Carregar variáveis do .env
// ================================
try
{
    Env.Load();
    Console.WriteLine("✅ Arquivo .env carregado com sucesso!");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Erro ao carregar .env: {ex.Message}");
}

// ================================
// 🔹 Obter string de conexão
// ================================
var dbConnection = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Mostrar no console para diagnóstico
Console.WriteLine("🔍 Conexão usada:");
Console.WriteLine(dbConnection);

// ================================
// 🔹 Configurar o DbContext
// ================================
builder.Services.AddDbContext<EventXContext>(options =>
    options.UseNpgsql(dbConnection)
           .EnableSensitiveDataLogging()   // logs detalhados
           .EnableDetailedErrors());       // mostra erro real do banco

// ================================
// 🔹 Identity (usuários e login)
// ================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<EventXContext>()
    .AddDefaultTokenProviders();

// ================================
// 🔹 Stripe
// ================================
StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
    ?? builder.Configuration["Stripe:SecretKey"];

// ================================
// 🔹 SignalR + MVC
// ================================
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ================================
// 🔹 Configuração de ambiente
// ================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ================================
// 🔹 Middleware principal
// ================================
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ================================
// 🔹 Rotas MVC e Hub do Chat
// ================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");

// ================================
// 🔹 Rodar aplicação
// ================================
app.Run();
