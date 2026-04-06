using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ProjetoEventX.Models;

namespace ProjetoEventX.ViewModels.Social
{
    public class PerfilSocialViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "o nome de exibição é obrigatório")]
        [StringLength(120)]
        public string NomeExibicao { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Bio { get; set; }

        public string? FotoPerfilUrl { get; set; }

        [StringLength(40)]
        public string TipoPerfil { get; set; } = "Convidado";

        [StringLength(120)]
        public string? Cidade { get; set; }

        [StringLength(120)]
        public string? Instagram { get; set; }

        [StringLength(200)]
        public string? Site { get; set; }

        public int TotalPosts { get; set; }
        public int TotalCurtidasRecebidas { get; set; }
        public List<SocialPost> ListaPosts { get; set; } = new();
        public List<Evento> EventosVinculados { get; set; } = new();
        public IFormFile? NovaFotoPerfil { get; set; }
    }
}
