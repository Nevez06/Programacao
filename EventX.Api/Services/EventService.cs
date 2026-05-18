using EventX.Api.Data;
using EventX.Api.DTOs.Events;
using EventX.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventX.Api.Services;

public sealed class EventService : IEventService
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public EventService(AppDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<EventDto>> GetMyEventsAsync(Guid organizerId, CancellationToken cancellationToken)
    {
        var events = await _dbContext.Events
            .AsNoTracking()
            .Where(x => x.OrganizerId == organizerId)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(cancellationToken);

        return events.Select(Map).ToList();
    }

    public async Task<EventDto?> GetMyEventByIdAsync(Guid organizerId, int eventId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == eventId && x.OrganizerId == organizerId, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<EventDto?> CreateEventAsync(Guid organizerId, CreateEventRequestDto request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (!IsPayloadValid(request.StartDate, request.EndDate, request.EstimatedAudience, request.EstimatedCost))
        {
            return null;
        }

        var entity = new Event
        {
            Name = name,
            Type = Normalize(request.Type),
            Description = Normalize(request.Description),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Location = Normalize(request.Location),
            CoverImageUrl = Normalize(request.CoverImageUrl),
            EstimatedAudience = request.EstimatedAudience,
            EstimatedCost = request.EstimatedCost,
            Status = request.Status,
            OrganizerId = organizerId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Events.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateForUserAsync(
            organizerId,
            "evento",
            "Evento criado",
            $"O evento \"{entity.Name}\" foi criado com sucesso.",
            $"/events/{entity.Id}",
            cancellationToken);

        return Map(entity);
    }

    public async Task<EventDto?> UpdateEventAsync(
        Guid organizerId,
        int eventId,
        UpdateEventRequestDto request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (!IsPayloadValid(request.StartDate, request.EndDate, request.EstimatedAudience, request.EstimatedCost))
        {
            return null;
        }

        var entity = await _dbContext.Events
            .FirstOrDefaultAsync(x => x.Id == eventId && x.OrganizerId == organizerId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Name = name;
        entity.Type = Normalize(request.Type);
        entity.Description = Normalize(request.Description);
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Location = Normalize(request.Location);
        entity.CoverImageUrl = Normalize(request.CoverImageUrl);
        entity.EstimatedAudience = request.EstimatedAudience;
        entity.EstimatedCost = request.EstimatedCost;
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<bool> DeleteEventAsync(Guid organizerId, int eventId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Events
            .FirstOrDefaultAsync(x => x.Id == eventId && x.OrganizerId == organizerId, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _dbContext.Events.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static EventDto Map(Event entity)
    {
        return new EventDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            Description = entity.Description,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Location = entity.Location,
            CoverImageUrl = entity.CoverImageUrl,
            EstimatedAudience = entity.EstimatedAudience,
            EstimatedCost = entity.EstimatedCost,
            Status = entity.Status.ToString(),
            OrganizerId = entity.OrganizerId
        };
    }

    private static bool IsPayloadValid(
        DateTime startDate,
        DateTime endDate,
        int? estimatedAudience,
        decimal? estimatedCost)
    {
        if (endDate < startDate)
        {
            return false;
        }

        if (estimatedAudience is < 0)
        {
            return false;
        }

        if (estimatedCost is < 0)
        {
            return false;
        }

        return true;
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
