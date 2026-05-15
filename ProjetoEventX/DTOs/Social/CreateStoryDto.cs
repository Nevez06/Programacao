using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Social
{
    public class CreateStoryDto
    {
        [Required]
        [StringLength(600)]
        public string MediaUrl { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Caption { get; set; }

        [StringLength(80)]
        public string? Theme { get; set; }

        public int? EventId { get; set; }
        public int? SharedPostId { get; set; }
    }
}