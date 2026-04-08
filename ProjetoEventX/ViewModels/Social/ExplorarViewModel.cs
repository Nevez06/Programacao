using ProjetoEventX.Models;

namespace ProjetoEventX.ViewModels.Social
{
    public class ExplorarViewModel
    {
        public List<FeedPostViewModel> PostsRecentes { get; set; } = new();
        public List<FeedPostViewModel> PostsPopulares { get; set; } = new();
        public List<ExplorarPerfilCardViewModel> PerfisOrganizadores { get; set; } = new();
        public List<ExplorarPerfilCardViewModel> PerfisFornecedores { get; set; } = new();
        public List<ExplorarEventoCardViewModel> EventosDestaque { get; set; } = new();
        public List<string> CategoriasDestaque { get; set; } = new();
        public int TotalPostsRecentes { get; set; }
        public int TotalPostsPopulares { get; set; }
    }
}
