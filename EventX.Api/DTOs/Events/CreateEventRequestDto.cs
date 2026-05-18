using System.ComponentModel.DataAnnotations;
using EventX.Api.Entities;

namespace EventX.Api.DTOs.Events;

public sealed class CreateEventRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Type { get; set; }

    [MaxLength(3000)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int? EstimatedAudience { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? EstimatedCost { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Draft;
}
