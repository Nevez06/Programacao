
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class Administracao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAdministrar { get; set; }

        public decimal ValorTotal { get; set; }

        public decimal Orcamento { get; set; }

        public int IdEvento { get; set; }

        [ForeignKey("IdEvento")]
        public required Evento Evento { get; set; }
    }
}