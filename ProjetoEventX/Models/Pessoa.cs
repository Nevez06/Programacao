using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.Models
{
    public class Pessoa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public required string Nome { get; set; }

        [StringLength(255)]
        public required string Endereco { get; set; }

        public string Telefone { get; set; } = string.Empty;

        [StringLength(14)]
        public required string Cpf { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public required string Email { get; set; }

        [StringLength(100)]
        public string Cidade { get; set; } = string.Empty;

        [StringLength(2)]
        public string UF { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Relacionamentos
        public Fornecedor? Fornecedor { get; set; }
        public Organizador? Organizador { get; set; }
        public Convidado? Convidado { get; set; }
        public ICollection<TarefaEvento> TarefasResponsaveis { get; set; } = new List<TarefaEvento>();
        public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
    }
}