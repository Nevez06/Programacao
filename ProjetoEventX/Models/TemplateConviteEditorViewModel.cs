namespace ProjetoEventX.Models
{
    public class TemplateConviteEditorViewModel
    {
        public int TemplateId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string NomeEvento { get; set; } = string.Empty;
        public string LayoutJson { get; set; } = string.Empty;
    }
}