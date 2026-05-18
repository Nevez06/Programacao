using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using ProjetoEventX.Security;
using ProjetoEventX.Services; // Certifique-se que GeminiEventService está aqui
using Stripe;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
const string FlutterApiCorsPolicy = "FlutterApiCorsPolicy";
const string ApiOrCookieAuthScheme = "ApiOrCookieAuthScheme";

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
var dbConnection = Env.GetString("DB_CONNECTION");

if (string.IsNullOrEmpty(dbConnection))
{
    dbConnection = builder.Configuration.GetConnectionString("DefaultConnection");
}
Console.WriteLine("🔍 Conexão FINAL:");
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

var jwtKey = Env.GetString("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? "EventX-Dev-Only-Replace-This-Key-Immediately-2026!";
var jwtIssuer = Env.GetString("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? "EventX";
var jwtAudience = Env.GetString("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? "EventX.Clients";
var jwtExpiresMinutes = int.TryParse(
    Env.GetString("JWT_EXPIRES_MINUTES") ?? builder.Configuration["Jwt:ExpiresMinutes"],
    out var parsedExpires)
    ? Math.Clamp(parsedExpires, 5, 1440)
    : 120;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = ApiOrCookieAuthScheme;
    options.DefaultAuthenticateScheme = ApiOrCookieAuthScheme;
    options.DefaultChallengeScheme = ApiOrCookieAuthScheme;
})
.AddPolicyScheme(ApiOrCookieAuthScheme, "JWT ou Cookie", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authorization = context.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorization)
            && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return JwtBearerDefaults.AuthenticationScheme;
        }

        return IdentityConstants.ApplicationScheme;
    };
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrWhiteSpace(accessToken)
                && (path.StartsWithSegments("/chatHub")
                    || path.StartsWithSegments("/hubs/notifications")))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

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
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<NotificationQueryService>();
builder.Services.AddScoped<FeedService>();
builder.Services.AddScoped<InviteService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<FornecedorPerformanceService>();
builder.Services.AddScoped<EventLogService>();
builder.Services.AddScoped<EventoSlugService>();

// ================================
// 🔹 CORS (Flutter/API)
// ================================
var allowedCorsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var normalizedAllowedOrigins = allowedCorsOrigins
    .Concat(new[] { "http://10.0.2.2", "https://10.0.2.2" })
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FlutterApiCorsPolicy, policy =>
    {
        if (normalizedAllowedOrigins.Length > 0)
        {
            policy.WithOrigins(normalizedAllowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
            return;
        }

        policy.SetIsOriginAllowed(origin =>
              {
                  if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                  {
                      return false;
                  }

                  return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                      || uri.Host.Equals("127.0.0.1")
                      || uri.Host.Equals("10.0.2.2");
              })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EventX API",
        Version = "v1",
        Description = "API principal do EventX para Flutter e integrações externas."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT no formato: Bearer {seu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EventXContext>();
    dbContext.Database.Migrate();
}

// ================================
// 🔹 Configuração de ambiente
// ================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

// ================================
// 🔹 Middleware principal
// ================================
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRouting();
app.UseCors(FlutterApiCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

// 🆕 O UseSession DEVE ficar DEPOIS de UseRouting e ANTES dos Controllers
app.UseSession();

// ================================
// 🔹 Rotas MVC e Hub do Chat
// ================================
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ProjetoEventX.Models.ChatHub>("/chatHub");
app.MapHub<ProjetoEventX.Models.NotificationsHub>("/hubs/notifications");

// ================================
// 🔹 Rodar aplicação
// ================================
app.Run();
