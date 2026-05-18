namespace EventX.Api.DTOs.Social;

public sealed class ToggleLikeResponseDto
{
    public int PostId { get; set; }
    public bool IsLiked { get; set; }
    public int LikesCount { get; set; }
}
