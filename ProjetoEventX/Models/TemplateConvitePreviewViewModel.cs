namespace ProjetoEventX.Models
{
    public class TemplateConvitePreviewViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public string NomeEvento { get; set; } = string.Empty;
        public string Estilo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string CorFundo { get; set; } = "#ffffff";
        public string CorTexto { get; set; } = "#333333";
        public string CorPrimaria { get; set; } = "#992008";
        public string Fonte { get; set; } = "Arial, sans-serif";

        public string Titulo { get; set; } = "Você está convidado!";
        public string Saudacao { get; set; } = "Olá Nome do Convidado,";
        public string Mensagem { get; set; } = "Você está cordialmente convidado para participar do nosso evento especial.";
        public string TextoBotao { get; set; } = "Confirmar Presença";

        public DateTime? DataEvento { get; set; }
        public string Horario { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;

        public string LayoutJson { get; set; } = string.Empty;
    }
}