using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Invites
{
    public class CreateInviteDto
    {
        [Required(ErrorMessage = "EventoId e obrigatorio.")]
        public int EventoId { get; set; }

        public int? ConvidadoId { get; set; }

        [StringLength(255, ErrorMessage = "Nome do convidado muito longo.")]
        public string? NomeConvidado { get; set; }

        [StringLength(255, ErrorMessage = "Email muito longo.")]
        [EmailAddress(ErrorMessage = "Email invalido.")]
        public string? EmailConvidado { get; set; }

        public int? TemplateId { get; set; }
        public bool EnviarAgora { get; set; }
        public string? MensagemOpcional { get; set; }
    }
}
