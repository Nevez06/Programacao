using ProjetoEventX.Models;

namespace ProjetoEventX.ViewModels.Social
{
    public class ExplorarViewModel
    {
        public List<FeedPostViewModel> PostsRecentes { get; set; } = new();
        public List<FeedPostViewModel> PostsPopulares { get; set; } = new();
        public List<PerfilSocial> PerfisOrganizadores { get; set; } = new();
        public List<PerfilSocial> PerfisFornecedores { get; set; } = new();
        public List<Evento> EventosDestaque { get; set; } = new();
    }
}
