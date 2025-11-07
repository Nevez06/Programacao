using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class Administracao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAdministrar { get; set; }

        [Range(0, 999999999)]
        public decimal ValorTotal { get; set; }

        [Range(0, 999999999)]
        public decimal Orcamento { get; set; }

        [Required]
        public int IdEvento { get; set; }

        [ForeignKey("IdEvento")]
        public Evento? Evento { get; set; } // ✅ Agora opcional — sem erro
    }
}
