using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Social
{
    public class UpdateSocialProfileDto
    {
        [StringLength(120, MinimumLength = 2)]
        public string? NomeExibicao { get; set; }

        [StringLength(40)]
        public string? Username { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(300)]
        public string? FotoPerfilUrl { get; set; }

        [StringLength(120)]
        public string? Cidade { get; set; }

        [StringLength(120)]
        public string? Instagram { get; set; }

        [StringLength(200)]
        public string? Site { get; set; }
    }
}