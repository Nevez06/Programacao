using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.Models
{
    public class Pessoa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Nome { get; set; }

        [StringLength(255)]
        public string Endereco { get; set; }

        public int Telefone { get; set; }

        [StringLength(14)]
        public string Cpf { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Relacionamentos
        public Fornecedor Fornecedor { get; set; }
        public Organizador Organizador { get; set; }
        public Convidado Convidado { get; set; }
        public ICollection<TarefaEvento> TarefasResponsaveis { get; set; } = new List<TarefaEvento>();
        public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
    }
}