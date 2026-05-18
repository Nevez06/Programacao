namespace ProjetoEventX.DTOs.Invites
{
    public class InviteListDto
    {
        public int Id { get; set; }
        public int EventoId { get; set; }
        public string EventoNome { get; set; } = string.Empty;
        public int ConvidadoId { get; set; }
        public string NomeConvidado { get; set; } = string.Empty;
        public string EmailConvidado { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Status { get; set; } = "Pendente";
        public DateTime DataConvite { get; set; }
        public bool CheckInRealizado { get; set; }
        public string? CodigoQr { get; set; }
    }
}
