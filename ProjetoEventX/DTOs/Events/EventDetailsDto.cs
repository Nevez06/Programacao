namespace ProjetoEventX.DTOs.Events
{
    public class EventDetailsDto
    {
        public int Id { get; set; }
        public string NomeEvento { get; set; } = string.Empty;
        public DateTime DataEvento { get; set; }
        public string DescricaoEvento { get; set; } = string.Empty;
        public string TipoEvento { get; set; } = string.Empty;
        public decimal CustoEstimado { get; set; }
        public string StatusEvento { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFim { get; set; } = string.Empty;
        public int PublicoEstimado { get; set; }
        public int OrganizadorId { get; set; }
        public int? LocalId { get; set; }
        public string? LocalNome { get; set; }
        public string? LocalEndereco { get; set; }
        public string? Slug { get; set; }
        public string? ImagemCapa { get; set; }
        public bool PermitirComentarios { get; set; }
        public bool PermitirEnvioFotos { get; set; }
        public bool PermitirCurtidas { get; set; }
        public bool AprovarFotosAntesPublicar { get; set; }
        public bool PermitirVisualizacaoMural { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
