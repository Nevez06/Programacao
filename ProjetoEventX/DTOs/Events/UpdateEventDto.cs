using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Events
{
    public class UpdateEventDto
    {
        [Required(ErrorMessage = "Nome do evento é obrigatório.")]
        [StringLength(255, ErrorMessage = "Nome do evento muito longo.")]
        public string NomeEvento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data do evento é obrigatória.")]
        public DateTime DataEvento { get; set; }

        [Required(ErrorMessage = "Descrição do evento é obrigatória.")]
        public string DescricaoEvento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tipo do evento é obrigatório.")]
        [StringLength(100, ErrorMessage = "Tipo do evento muito longo.")]
        public string TipoEvento { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Status do evento muito longo.")]
        public string? StatusEvento { get; set; }

        [Required(ErrorMessage = "Hora de início é obrigatória.")]
        [StringLength(5, ErrorMessage = "Hora de início inválida.")]
        public string HoraInicio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hora de fim é obrigatória.")]
        [StringLength(5, ErrorMessage = "Hora de fim inválida.")]
        public string HoraFim { get; set; } = string.Empty;

        public int PublicoEstimado { get; set; }
        public decimal CustoEstimado { get; set; }
        public int? LocalId { get; set; }

        [StringLength(500, ErrorMessage = "URL da imagem de capa muito longa.")]
        public string? ImagemCapa { get; set; }

        public bool PermitirComentarios { get; set; } = true;
        public bool PermitirEnvioFotos { get; set; } = true;
        public bool PermitirCurtidas { get; set; } = true;
        public bool AprovarFotosAntesPublicar { get; set; }
        public bool PermitirVisualizacaoMural { get; set; } = true;
    }
}
