using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Social
{
    public class CreateHighlightDto
    {
        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string? CapaUrl { get; set; }

        public List<int> StoryIds { get; set; } = new();
    }
}