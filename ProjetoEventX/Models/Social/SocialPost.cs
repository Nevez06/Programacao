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

        [StringLength(80)]
        public string? TipoConteudo { get; set; }

        [StringLength(140)]
        public string? Localizacao { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;
        public bool IsPinned { get; set; }
        public int? PinnedOrder { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
        public bool HideLikesCount { get; set; }
        public bool HideSharesCount { get; set; }
        public bool CommentsEnabled { get; set; } = true;

        public ICollection<SocialCurtida> Curtidas { get; set; } = new List<SocialCurtida>();
        public ICollection<SocialComentario> Comentarios { get; set; } = new List<SocialComentario>();
        public ICollection<SocialPostSalvo> Salvos { get; set; } = new List<SocialPostSalvo>();
    }
}
