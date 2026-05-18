namespace EventX.Api.DTOs.System;

public sealed class HealthCheckResponseDto
{
    public string Status { get; set; } = "Healthy";
    public string Service { get; set; } = "EventX.Api";
    public string Environment { get; set; } = string.Empty;
    public DateTime UtcNow { get; set; }
    public bool DatabaseConfigured { get; set; }
    public bool DatabaseConnected { get; set; }
}
