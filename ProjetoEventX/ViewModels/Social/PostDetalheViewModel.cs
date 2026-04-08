using ProjetoEventX.Models;

namespace ProjetoEventX.ViewModels.Social
{
    public class PostDetalheViewModel
    {
        public SocialPost Post { get; set; } = null!;
        public PerfilSocial Perfil { get; set; } = null!;
        public List<SocialComentario> ListaComentarios { get; set; } = new();
        public int TotalCurtidas { get; set; }
        public bool UsuarioCurtiu { get; set; }
        public bool UsuarioSalvou { get; set; }
        public Evento? EventoRelacionado { get; set; }
        public string NomeAutor { get; set; } = string.Empty;
        public string TipoPerfil { get; set; } = "Convidado";
        public string? Cidade { get; set; }
        public List<SocialStatusItemViewModel> StatusAutorAtivos { get; set; } = new();
    }
}
