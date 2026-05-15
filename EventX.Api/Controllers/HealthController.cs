using Microsoft.AspNetCore.Mvc;
using EventX.Api.DTOs.System;
using EventX.Api.Services;

namespace EventX.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;

    public HealthController(IHealthService healthService)
    {
        _healthService = healthService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponseDto), StatusCodes.Status200OK)]
    public ActionResult<HealthCheckResponseDto> Get()
    {
        return Ok(_healthService.GetStatus());
    }
}
