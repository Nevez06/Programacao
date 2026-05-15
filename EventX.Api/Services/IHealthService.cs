using EventX.Api.DTOs.System;

namespace EventX.Api.Services;

public interface IHealthService
{
    HealthCheckResponseDto GetStatus();
}
