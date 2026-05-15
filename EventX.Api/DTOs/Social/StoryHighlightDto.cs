namespace EventX.Api.DTOs.Social;

public sealed class StoryHighlightDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CapaUrl { get; set; }
    public int TotalStories { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<StoryDto> Stories { get; set; } = [];
}
