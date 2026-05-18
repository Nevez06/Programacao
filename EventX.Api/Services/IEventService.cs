using EventX.Api.DTOs.Events;

namespace EventX.Api.Services;

public interface IEventService
{
    Task<IReadOnlyList<EventDto>> GetMyEventsAsync(Guid organizerId, CancellationToken cancellationToken);
    Task<EventDto?> GetMyEventByIdAsync(Guid organizerId, int eventId, CancellationToken cancellationToken);
    Task<EventDto?> CreateEventAsync(Guid organizerId, CreateEventRequestDto request, CancellationToken cancellationToken);
    Task<EventDto?> UpdateEventAsync(Guid organizerId, int eventId, UpdateEventRequestDto request, CancellationToken cancellationToken);
    Task<bool> DeleteEventAsync(Guid organizerId, int eventId, CancellationToken cancellationToken);
}
