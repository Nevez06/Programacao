using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(120, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string TipoUsuario { get; set; } = "Organizador";

        [StringLength(120)]
        public string? UserName { get; set; }

        [StringLength(14)]
        public string? Cpf { get; set; }

        [StringLength(18)]
        public string? Cnpj { get; set; }

        [StringLength(255)]
        public string? Endereco { get; set; }

        [StringLength(30)]
        public string? Telefone { get; set; }

        [StringLength(100)]
        public string? Cidade { get; set; }

        [StringLength(2)]
        public string? Uf { get; set; }

        [StringLength(255)]
        public string? TipoServico { get; set; }
    }
}