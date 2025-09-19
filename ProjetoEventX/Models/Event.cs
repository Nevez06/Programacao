using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class Evento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public required string NomeEvento { get; set; }

        [Required]
        public DateTime DataEvento { get; set; }

        public required string DescricaoEvento { get; set; }

        [StringLength(100)]
        public required string TipoEvento { get; set; }

        public decimal CustoEstimado { get; set; } = 0.0m;

        [StringLength(50)]
        public string StatusEvento { get; set; } = "Planejado";

        public int? IdTemplateEvento { get; set; }

        [ForeignKey("IdTemplateEvento")]
        public TemplateEvento? TemplateEvento { get; set; }

        [StringLength(5)]
        public required string HoraInicio { get; set; }

        [StringLength(5)]
        public required string HoraFim { get; set; }

        public int PublicoEstimado { get; set; }

        [Required]
        public int OrganizadorId { get; set; }

        [ForeignKey("OrganizadorId")]
        public Organizador? Organizador { get; set; }

        public int? LocalId { get; set; }

        [ForeignKey("LocalId")]
        public Local? Local { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Relacionamentos
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public ICollection<TarefaEvento> TarefasEventos { get; set; } = new List<TarefaEvento>();
        public ICollection<AssistenteVirtual> AssistentesVirtuais { get; set; } = new List<AssistenteVirtual>();
        public ICollection<ListaConvidado> ListasConvidados { get; set; } = new List<ListaConvidado>();
        public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<Administracao> Administracoes { get; set; } = new List<Administracao>();
    }
}