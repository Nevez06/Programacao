namespace ProjetoEventX.ViewModels.Social
{
    public class ExplorarVisualItemViewModel
    {
        public string Tipo { get; set; } = "post";
        public string Titulo { get; set; } = string.Empty;
        public string? Subtitulo { get; set; }
        public string ImagemUrl { get; set; } = "/uploads/social/defaults/default-post.svg";
        public string Link { get; set; } = "#";
        public string Badge { get; set; } = "post";
        public int Score { get; set; }
        public decimal ScoreRelevancia { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public string? Categoria { get; set; }
        public int TotalCurtidas { get; set; }
        public int TotalComentarios { get; set; }
        public int TotalVisualizacoes { get; set; }
        public int TotalCompartilhamentos { get; set; }
        public bool PossuiImagemValida { get; set; }
        public bool DestaqueManual { get; set; }
        public bool Destaque { get; set; }
        public bool IsVideo { get; set; }
        public bool IsMulti { get; set; }
    }
}
