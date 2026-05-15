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
        public string? AvatarUrl { get; set; }

        [StringLength(40)]
        public string TipoPerfil { get; set; } = "Convidado";
        public string CategoriaPerfil { get; set; } = "Convidado";

        [StringLength(120)]
        public string? Cidade { get; set; }
        public string? Link { get; set; }
        public string? NomeReal { get; set; }
        [StringLength(40)]
        public string? Username { get; set; }
        public string? NomeUsuario { get; set; }

        [StringLength(120)]
        public string? Instagram { get; set; }

        [StringLength(200)]
        public string? Site { get; set; }

        public int TotalPosts { get; set; }
        public int TotalCurtidasRecebidas { get; set; }
        public int TotalStoriesAtivos { get; set; }
        public int TotalVisualizacoesStoriesRecentes { get; set; }
        public int TotalDestaques { get; set; }
        public int TotalEngajamentoStories { get; set; }
        public int TotalEventosRelacionados { get; set; }
        public int TotalSeguidores { get; set; }
        public int TotalSeguindo { get; set; }
        public bool UsuarioSegue { get; set; }
        public bool EhPerfilDoUsuarioLogado { get; set; }
        public string? UsernamePublico { get; set; }
        public List<SocialPost> ListaPosts { get; set; } = new();
        public List<SocialPost> ListaVideos { get; set; } = new();
        public List<SocialPost> ListaMarcados { get; set; } = new();
        public List<Evento> ListaEventos { get; set; } = new();
        public List<FeedPostViewModel> ListaSalvos { get; set; } = new();
        public List<Evento> EventosVinculados { get; set; } = new();
        public List<SocialStatusItemViewModel> StoriesAtivos { get; set; } = new();
        public List<StoryHighlightViewModel> Highlights { get; set; } = new();
        public List<StoryHighlightViewModel> Destaques { get; set; } = new();
        public List<FeedPostViewModel> PostsSalvos { get; set; } = new();
        public IFormFile? NovaFotoPerfil { get; set; }
    }
}
