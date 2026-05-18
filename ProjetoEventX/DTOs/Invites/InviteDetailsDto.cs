namespace ProjetoEventX.DTOs.Invites
{
    public class InviteDetailsDto : InviteListDto
    {
        public DateTime? DataEvento { get; set; }
        public string? HoraInicio { get; set; }
        public string? HoraFim { get; set; }
        public string? LocalNome { get; set; }
        public string? LocalEndereco { get; set; }
        public int? TemplateId { get; set; }
        public string? TemplateNome { get; set; }
    }
}
