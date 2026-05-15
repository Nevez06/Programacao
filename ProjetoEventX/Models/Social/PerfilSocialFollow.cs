using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class PerfilSocialFollow
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SeguidorPerfilId { get; set; }

        [ForeignKey(nameof(SeguidorPerfilId))]
        public PerfilSocial? SeguidorPerfil { get; set; }

        [Required]
        public int SeguindoPerfilId { get; set; }

        [ForeignKey(nameof(SeguindoPerfilId))]
        public PerfilSocial? SeguindoPerfil { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
