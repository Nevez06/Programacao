namespace EventX.Api.DTOs.Users;

public sealed class UserProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
