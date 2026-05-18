namespace ProjetoEventX.ViewModels.Social
{
    public class ExplorarTemplateCardViewModel
    {
        public int TemplateId { get; set; }
        public int? EventoId { get; set; }
        public string NomeTemplate { get; set; } = string.Empty;
        public string Categoria { get; set; } = "template";
        public string Estilo { get; set; } = "editor visual";
        public string ThumbnailUrl { get; set; } = "/uploads/social/defaults/default-post.svg";
        public DateTime AtualizadoEm { get; set; }
        public string LinkEditor { get; set; } = "#";
    }
}
