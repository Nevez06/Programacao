using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class SocialPost
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

        public int? EventoId { get; set; }

        [ForeignKey(nameof(EventoId))]
        public Evento? Evento { get; set; }

        [StringLength(120)]
        public string? Titulo { get; set; }

        [Required]
        [StringLength(2000)]
        public string Legenda { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string ImagemUrl { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Categoria { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;

        public ICollection<SocialCurtida> Curtidas { get; set; } = new List<SocialCurtida>();
        public ICollection<SocialComentario> Comentarios { get; set; } = new List<SocialComentario>();
    }
}
