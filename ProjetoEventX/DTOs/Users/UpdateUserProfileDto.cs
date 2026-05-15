using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Users
{
    public class UpdateUserProfileDto
    {
        [StringLength(255, ErrorMessage = "Nome muito longo.")]
        public string? Nome { get; set; }

        [StringLength(500, ErrorMessage = "URL da foto muito longa.")]
        public string? FotoUrl { get; set; }

        [StringLength(50, ErrorMessage = "Telefone muito longo.")]
        public string? Telefone { get; set; }

        [StringLength(100, ErrorMessage = "Cidade muito longa.")]
        public string? Cidade { get; set; }

        [StringLength(2, ErrorMessage = "Estado deve ter 2 caracteres.")]
        public string? Estado { get; set; }

        [StringLength(255, ErrorMessage = "Endereco muito longo.")]
        public string? Endereco { get; set; }

        [StringLength(14, ErrorMessage = "CPF muito longo.")]
        public string? Cpf { get; set; }
    }
}
