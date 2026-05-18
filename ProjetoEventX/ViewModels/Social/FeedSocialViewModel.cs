using ProjetoEventX.Models;

namespace ProjetoEventX.ViewModels.Social
{
    public class FeedSocialViewModel
    {
        public List<FeedPostViewModel> Posts { get; set; } = new();
        public List<SocialStatusItemViewModel> Stories { get; set; } = new();
        public List<SocialStatusItemViewModel> StoriesEmAlta { get; set; } = new();
        public List<PerfilSocial> PerfisDestaque { get; set; } = new();
        public List<Evento> EventosEmAlta { get; set; } = new();
        public PerfilSocial? PerfilAtual { get; set; }
        public string OrdenacaoAtual { get; set; } = "recentes";
        public string InsightSemana { get; set; } = string.Empty;
        public List<PerfilSocial> FornecedoresEmAlta { get; set; } = new();
        public List<string> CategoriasTendencia { get; set; } = new();
    }
}
