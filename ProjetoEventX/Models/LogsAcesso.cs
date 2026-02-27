using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class LogsAcesso
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(45)] // IPv4 ou IPv6 compactado
        public string EnderecoIP { get; set; } = string.Empty;

        [Required]
        public DateTime DataAcesso { get; set; }

        [Required]
        [StringLength(256)]
        public string Usuario { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string UrlAcesso { get; set; } = string.Empty;

        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty;

        [StringLength(50)]
        public string? TipoAcesso { get; set; }

        public bool? AcessoBloqueado { get; set; }

        [StringLength(500)]
        public string? MotivoBloqueio { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}