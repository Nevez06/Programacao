namespace ProjetoEventX.ViewModels.Social
{
    public class ExplorarEventoCardViewModel
    {
        public int EventoId { get; set; }
        public string NomeEvento { get; set; } = string.Empty;
        public string TipoEvento { get; set; } = string.Empty;
        public DateTime DataEvento { get; set; }
        public string? ImagemCapa { get; set; }
        public int TotalPostsRelacionados { get; set; }
    }
}
