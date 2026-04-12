using ProjetoEventX.Models;

namespace ProjetoEventX.ViewModels.Social
{
    public class ExplorarViewModel
    {
        public string? Busca { get; set; }
        public string? CategoriaAtiva { get; set; }
        public List<FeedPostViewModel> PostsRecentes { get; set; } = new();
        public List<FeedPostViewModel> PostsPopulares { get; set; } = new();
        public List<ExplorarPerfilCardViewModel> OrganizadoresDestaque { get; set; } = new();
        public List<ExplorarPerfilCardViewModel> FornecedoresPopulares { get; set; } = new();
        public List<ExplorarEventoCardViewModel> EventosEmAlta { get; set; } = new();
        public List<ExplorarTemplateCardViewModel> TemplatesEmAlta { get; set; } = new();
        public List<ExplorarStoryCardViewModel> StoriesDestaque { get; set; } = new();
        public List<ExplorarInspiracaoCardViewModel> Inspiracoes { get; set; } = new();
        public List<ExplorarVisualItemViewModel> DescobertaVisual { get; set; } = new();
        public List<string> CategoriasDestaque { get; set; } = new();
        public List<string> SugestoesBusca { get; set; } = new();
        public int TotalPostsRecentes { get; set; }
        public int TotalPostsPopulares { get; set; }
        public int TotalEventosEmAlta { get; set; }
        public int TotalFornecedoresPopulares { get; set; }
        public int TotalTemplatesEmAlta { get; set; }
        public int TotalStoriesDestaque { get; set; }
    }
}
