using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class SocialStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [Required]
        public int PerfilSocialId { get; set; }

        [ForeignKey(nameof(PerfilSocialId))]
        public PerfilSocial? PerfilSocial { get; set; }

        [Required]
        [StringLength(300)]
        public string ImagemUrl { get; set; } = string.Empty;

        [StringLength(180)]
        public string? TextoOverlay { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime ExpiraEm { get; set; } = DateTime.UtcNow.AddHours(24);
        public bool Ativo { get; set; } = true;

        public ICollection<SocialStatusVisualizacao> Visualizacoes { get; set; } = new List<SocialStatusVisualizacao>();
    }
}
