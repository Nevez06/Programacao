namespace ProjetoEventX.ViewModels.Social
{
    public class ExplorarPerfilCardViewModel
    {
        public int PerfilId { get; set; }
        public string NomeExibicao { get; set; } = string.Empty;
        public string TipoPerfil { get; set; } = "Convidado";
        public string? Cidade { get; set; }
        public string? Bio { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public int TotalPosts { get; set; }
    }
}
