using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Hubs;
using ProjetoEventX.Models;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Carregar variáveis do .env (se existir)
try
{
    Env.Load();
}
catch
{
    // Arquivo .env não encontrado, usar appsettings.json
}

// DbContext com conexão do .env ou appsettings.json
var dbConnection = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EventXContext>(options =>
    options.UseNpgsql(dbConnection));

builder.Services.AddDbContext<SimpleEventXContext>(options =>
    options.UseNpgsql(dbConnection));

// Identity
builder.Services.AddIdentity<IdentityUser<int>, IdentityRole<int>>()
    .AddEntityFrameworkStores<EventXContext>()
    .AddDefaultTokenProviders();

// Stripe
StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
    ?? builder.Configuration["Stripe:SecretKey"];

// SignalR
builder.Services.AddSignalR();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ChatHub>("/chatHub");

app.Run();