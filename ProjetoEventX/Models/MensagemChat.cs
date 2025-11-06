<<<<<<< HEAD
﻿namespace ProjetoEventX.Models
{
    public class MensagemChat
    {
        public int Id { get; set; }
        public int RemetenteId { get; set; } // ID do Organizador que envia
        public int DestinatarioId { get; set; } // ID do Convidado ou Fornecedor
        public required string TipoDestinatario { get; set; } // "Convidado" ou "Fornecedor"
        public required string Conteudo { get; set; } // Texto da mensagem
        public DateTime DataEnvio { get; set; }
        public int EventoId { get; set; } // Relacionado ao Evento
        public bool EhRespostaAssistente { get; set; } // Indica se é resposta do assistente virtual

        // Relacionamentos
        public required Pessoa Remetente { get; set; } // Organizador
        public required Pessoa Destinatario { get; set; } // Convidado ou Fornecedor
        public required Evento Evento { get; set; }
=======
﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class MensagemChat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventoId { get; set; }

        [ForeignKey("EventoId")]
        public Evento Evento { get; set; }

        [Required]
        public int ConvidadoId { get; set; }

        [ForeignKey("ConvidadoId")]
        public Convidado Convidado { get; set; }

        [Required]
        public string Mensagem { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.Now;
>>>>>>> 9c557d6 (feat: adiciona controladores e atualiza modelos e views)
    }
}