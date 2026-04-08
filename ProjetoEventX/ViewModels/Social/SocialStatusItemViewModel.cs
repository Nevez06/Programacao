namespace ProjetoEventX.ViewModels.Social
{
    public class SocialStatusItemViewModel
    {
        public int Id { get; set; }
        public int PerfilId { get; set; }
        public string NomePerfil { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public string ImagemUrl { get; set; } = string.Empty;
        public string? TextoOverlay { get; set; }
        public DateTime CriadoEm { get; set; }
        public bool Visualizado { get; set; }
    }
}
