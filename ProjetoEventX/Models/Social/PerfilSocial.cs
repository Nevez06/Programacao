using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class PerfilSocial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [Required]
        [StringLength(120)]
        public string NomeExibicao { get; set; } = string.Empty;

        [StringLength(40)]
        public string? Username { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(300)]
        public string? FotoPerfilUrl { get; set; }

        [StringLength(40)]
        public string TipoPerfil { get; set; } = "Convidado";

        [StringLength(120)]
        public string? Cidade { get; set; }

        [StringLength(120)]
        public string? Instagram { get; set; }

        [StringLength(200)]
        public string? Site { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

        public ICollection<SocialPost> Posts { get; set; } = new List<SocialPost>();
    }
}
