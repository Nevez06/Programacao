using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Invites
{
    public class RsvpResponseDto
    {
        [Required(ErrorMessage = "Resposta e obrigatoria.")]
        [StringLength(30, ErrorMessage = "Resposta invalida.")]
        public string Resposta { get; set; } = string.Empty;
    }
}
