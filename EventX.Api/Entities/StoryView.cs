namespace EventX.Api.Entities;

public sealed class StoryView
{
    public int Id { get; set; }

    public int StoryId { get; set; }
    public Story Story { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}
