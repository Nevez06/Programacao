namespace EventX.Api.DTOs.Ranking;

public sealed class RankingSuppliersQueryDto
{
    public string? Category { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public int Take { get; set; } = 120;
}
