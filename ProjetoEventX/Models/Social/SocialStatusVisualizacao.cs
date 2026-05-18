using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class SocialStatusVisualizacao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SocialStatusId { get; set; }

        [ForeignKey(nameof(SocialStatusId))]
        public SocialStatus? SocialStatus { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public DateTime VisualizadoEm { get; set; } = DateTime.UtcNow;
    }
}
