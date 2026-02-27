using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TipoEntidade { get; set; } = string.Empty;

        [Required]
        public int EntidadeId { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoAcao { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, LOGIN, LOGOUT, VIEW

        [Required]
        [StringLength(256)]
        public string Usuario { get; set; } = string.Empty;

        [Required]
        public DateTime DataAcao { get; set; } = DateTime.UtcNow;

        [StringLength(45)]
        public string? EnderecoIP { get; set; }

        [StringLength(500)]
        public string? Descricao { get; set; }

        [Column(TypeName = "TEXT")]
        public string? DadosAntigos { get; set; } // JSON dos dados anteriores

        [Column(TypeName = "TEXT")]
        public string? DadosNovos { get; set; } // JSON dos novos dados

        [StringLength(50)]
        public string? Navegador { get; set; }

        [StringLength(50)]
        public string? SistemaOperacional { get; set; }

        public bool? Sucesso { get; set; } = true;

        [StringLength(1000)]
        public string? MensagemErro { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}