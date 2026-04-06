namespace ProjetoEventX.ViewModels.Social
{
    public class FeedPostViewModel
    {
        public int PostId { get; set; }
        public string NomeAutor { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public string Legenda { get; set; } = string.Empty;
        public string ImagemUrl { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public DateTime DataCriacao { get; set; }
        public int TotalCurtidas { get; set; }
        public int TotalComentarios { get; set; }
        public bool UsuarioCurtiu { get; set; }
        public int? EventoId { get; set; }
        public string? NomeEvento { get; set; }
        public int PerfilId { get; set; }
    }
}
