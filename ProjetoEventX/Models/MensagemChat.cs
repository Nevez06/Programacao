using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class MensagemChat
    {
        [Key]
        public int Id { get; set; }

        // Mapeamento para Remetente e Destinatario
        [Required]
        public int RemetenteId { get; set; }

        [Required]
        public int DestinatarioId { get; set; }

        [Required]
        public string TipoDestinatario { get; set; } = string.Empty;

        [Required]
        public string Conteudo { get; set; } = string.Empty;

        [Required]
        public int EventoId { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.Now;

        public bool EhRespostaAssistente { get; set; }

        [ForeignKey(nameof(RemetenteId))]
        public Pessoa? Remetente { get; set; }

        [ForeignKey(nameof(DestinatarioId))]
        public Pessoa? Destinatario { get; set; }

        [ForeignKey(nameof(EventoId))]
        public Evento? Evento { get; set; }

        // Propriedades auxiliares para a view (não mapeadas no banco)
        [NotMapped]
        public int ConvidadoId => DestinatarioId;  // para view usar ConvidadoId

        [NotMapped]
        public Pessoa? Convidado => Destinatario;  // para view usar Convidado

        [NotMapped]
        public string Mensagem => Conteudo;         // para view usar Mensagem
    }
}
