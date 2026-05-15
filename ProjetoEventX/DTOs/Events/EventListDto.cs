namespace ProjetoEventX.DTOs.Events
{
    public class EventListDto
    {
        public int Id { get; set; }
        public string NomeEvento { get; set; } = string.Empty;
        public DateTime DataEvento { get; set; }
        public string TipoEvento { get; set; } = string.Empty;
        public string StatusEvento { get; set; } = string.Empty;
        public int PublicoEstimado { get; set; }
        public decimal CustoEstimado { get; set; }
        public int? LocalId { get; set; }
        public string? LocalNome { get; set; }
        public string? Slug { get; set; }
        public string? ImagemCapa { get; set; }
    }
}
