namespace ProjetoEventX.Models
{
    public class ConviteCentralViewModel
    {
        public int EventoId { get; set; }
        public string NomeEvento { get; set; } = string.Empty;
        public List<TemplateCatalogItemViewModel> Templates { get; set; } = new();
        public List<ConviteRascunho> RascunhosRecentes { get; set; } = new();
    }

    public class TemplateCatalogItemViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Estilo { get; set; } = "Editor Visual";
        public string Thumbnail { get; set; } = string.Empty;
        public string LayoutJson { get; set; } = string.Empty;
        public bool Destaque { get; set; }
    }
}
