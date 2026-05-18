namespace EventX.Api.DTOs.Events;

public sealed class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public string? CoverImageUrl { get; set; }
    public int? EstimatedAudience { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid OrganizerId { get; set; }
}
