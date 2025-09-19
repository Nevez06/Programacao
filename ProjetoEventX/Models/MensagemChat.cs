namespace ProjetoEventX.Models
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
    }
}