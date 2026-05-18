using EventX.Api.DTOs.System;
using EventX.Api.Data;

namespace EventX.Api.Services;

public sealed class HealthService : IHealthService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly AppDbContext _dbContext;

    public HealthService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        AppDbContext dbContext)
    {
        _configuration = configuration;
        _environment = environment;
        _dbContext = dbContext;
    }

    public HealthCheckResponseDto GetStatus()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var databaseConfigured = !string.IsNullOrWhiteSpace(connectionString);
        var databaseConnected = false;

        if (databaseConfigured)
        {
            try
            {
                databaseConnected = _dbContext.Database.CanConnect();
            }
            catch
            {
                databaseConnected = false;
            }
        }

        return new HealthCheckResponseDto
        {
            Status = databaseConnected ? "Healthy" : "Degraded",
            Service = "EventX.Api",
            Environment = _environment.EnvironmentName,
            UtcNow = DateTime.UtcNow,
            DatabaseConfigured = databaseConfigured,
            DatabaseConnected = databaseConnected
        };
    }
}
