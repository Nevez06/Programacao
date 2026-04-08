using ProjetoEventX.Models;

namespace ProjetoEventX.ViewModels.Social
{
    public class FeedSocialViewModel
    {
        public List<FeedPostViewModel> Posts { get; set; } = new();
        public List<SocialStatusItemViewModel> Stories { get; set; } = new();
        public List<PerfilSocial> PerfisDestaque { get; set; } = new();
        public List<Evento> EventosEmAlta { get; set; } = new();
    }
}
