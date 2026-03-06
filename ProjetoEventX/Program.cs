using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using ProjetoEventX.Services; // Certifique-se que GeminiEventService está aqui
using ProjetoEventX.Security;
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

Console.WriteLine("🔍 Conexão usada:");
Console.WriteLine(dbConnection);

// ================================
// 🔹 Configurar o DbContext
// ================================
builder.Services.AddDbContext<EventXContext>(options =>
    options.UseNpgsql(dbConnection)
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors());

// ================================
// 🔹 Identity (usuários e login)
// ================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<EventXContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

// ================================
// 🔹 SESSÃO (Para Limitações de Chat) - NOVO 🆕
// ================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Sessão dura 30min
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ================================
// 🔹 HttpClient e Serviços de IA - ATUALIZADO 🆕
// ================================
// Registra o HttpClient
builder.Services.AddHttpClient();

// Registra os serviços de IA
builder.Services.AddHttpClient<GeminiEventService>();
builder.Services.AddScoped<EventBotService>();
builder.Services.AddScoped<ProjetoEventX.Services.EmailService>();

// ================================
// 🔹 SERVIÇOS DE SEGURANÇA - NOVO 🆕
// ================================
builder.Services.AddScoped<AuditoriaService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SecurityActionFilter>();

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
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 🆕 O UseSession DEVE ficar DEPOIS de UseRouting e ANTES dos Controllers
app.UseSession();

// ================================
// 🔹 Rotas MVC e Hub do Chat
// ================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ProjetoEventX.Models.ChatHub>("/chatHub");

// ================================
// 🔹 Rodar aplicação
// ================================
app.Run();