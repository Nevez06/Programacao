using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        [StringLength(256, ErrorMessage = "Email muito longo.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Senha é obrigatória.")]
        public string Password { get; set; } = string.Empty;
    }
}
