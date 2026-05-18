namespace ProjetoEventX.ViewModels.Social
{
    public class StoryViewerViewModel
    {
        public string NomeExibicao { get; set; } = string.Empty;
        public string FotoPerfilUrl { get; set; } = string.Empty;
        public string TipoPerfil { get; set; } = "Convidado";
        public DateTime ViewedAt { get; set; }
    }
}
