namespace ProjetoEventX.ViewModels.Social
{
    public class ExplorarStoryCardViewModel
    {
        public int StoryId { get; set; }
        public int PerfilId { get; set; }
        public string NomePerfil { get; set; } = "Perfil";
        public string? FotoPerfilUrl { get; set; }
        public string CapaUrl { get; set; } = "/uploads/social/defaults/default-post.svg";
        public DateTime CriadoEm { get; set; }
        public int Visualizacoes { get; set; }
        public int Reacoes { get; set; }
    }
}
