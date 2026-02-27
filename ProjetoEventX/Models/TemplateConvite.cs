using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class TemplateConvite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NomeTemplate { get; set; } = string.Empty;

        [Required]
        public int EventoId { get; set; }

        [ForeignKey("EventoId")]
        public required Evento Evento { get; set; }

        [Required]
        public int OrganizadorId { get; set; }

        [ForeignKey("OrganizadorId")]
        public required Organizador Organizador { get; set; }

        [Required]
        [StringLength(100)]
        public string TituloConvite { get; set; } = "Você está convidado!";

        [Required]
        [Column(TypeName = "text")]
        public string MensagemPrincipal { get; set; } = "Você está cordialmente convidado para participar do nosso evento especial.";

        [StringLength(500)]
        public string? MensagemSecundaria { get; set; }

        [StringLength(100)]
        public string CorFundo { get; set; } = "#ffffff";

        [StringLength(100)]
        public string CorTexto { get; set; } = "#333333";

        [StringLength(100)]
        public string CorPrimaria { get; set; } = "#007bff";

        [StringLength(100)]
        public string? FonteTitulo { get; set; } = "Arial, sans-serif";

        [StringLength(100)]
        public string? FonteTexto { get; set; } = "Arial, sans-serif";

        public int TamanhoFonteTitulo { get; set; } = 32;

        public int TamanhoFonteTexto { get; set; } = 16;

        public bool MostrarLogo { get; set; } = true;

        public bool MostrarFotoEvento { get; set; } = true;

        public bool MostrarMapa { get; set; } = true;

        public bool MostrarQRCode { get; set; } = false;

        [StringLength(500)]
        public string? ImagemCabecalho { get; set; }

        [StringLength(500)]
        public string? ImagemRodape { get; set; }

        [StringLength(100)]
        public string EstiloLayout { get; set; } = "Moderno";

        [StringLength(1000)]
        public string? CSSPersonalizado { get; set; }

        public bool Ativo { get; set; } = true;

        public bool PadraoSistema { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Método para gerar o HTML do convite com base no template
        public string GerarHTMLConvite(string nomeConvidado, string linkConfirmacao)
        {
            var html = @"
<!DOCTYPE html>
<html lang='pt-br'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>" + TituloConvite + @"</title>
    <style>
        body {
            font-family: " + FonteTexto + @";
            background-color: " + CorFundo + @";
            color: " + CorTexto + @";
            margin: 0;
            padding: 20px;
            line-height: 1.6;
        }
        .convite-container {
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 10px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            overflow: hidden;
        }
        .convite-header {
            background: " + CorPrimaria + @";
            color: white;
            padding: 30px;
            text-align: center;
        }
        .convite-title {
            font-family: " + FonteTitulo + @";
            font-size: " + TamanhoFonteTitulo + @"px;
            margin: 0 0 10px 0;
        }
        .convite-content {
            padding: 30px;
        }
        .convite-mensagem {
            font-size: " + TamanhoFonteTexto + @"px;
            margin-bottom: 20px;
        }
        .convite-detalhes {
            background: #f8f9fa;
            padding: 20px;
            border-radius: 5px;
            margin: 20px 0;
        }
        .btn-confirmar {
            display: inline-block;
            background: " + CorPrimaria + @";
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 10px 0;
        }
        .convite-footer {
            background: #f8f9fa;
            padding: 20px;
            text-align: center;
            font-size: 14px;
            color: #666;
        }
        " + (CSSPersonalizado ?? "") + @"
    </style>
</head>
<body>
    <div class='convite-container'>
        <div class='convite-header'>
            <h1 class='convite-title'>" + TituloConvite + @"</h1>
        </div>
        
        <div class='convite-content'>
            <div class='convite-mensagem'>
                <p>Olá <strong>" + nomeConvidado + @"</strong>,</p>
                <p>" + MensagemPrincipal + @"</p>
                " + (!string.IsNullOrEmpty(MensagemSecundaria) ? "<p>" + MensagemSecundaria + "</p>" : "") + @"
            </div>
            
            <div class='convite-detalhes'>
                <h3>Detalhes do Evento</h3>
                <p><strong>Evento:</strong> " + Evento.NomeEvento + @"</p>
                <p><strong>Data:</strong> " + Evento.DataEvento.ToString("dd/MM/yyyy") + @"</p>
                <p><strong>Horário:</strong> " + Evento.HoraInicio + " às " + Evento.HoraFim + @"</p>
                <p><strong>Local:</strong> " + Evento.Local?.NomeLocal + @"</p>
                <p><strong>Endereço:</strong> " + Evento.Local?.EnderecoLocal + @"</p>
            </div>
            
            <div style='text-align: center;'>
                <a href='" + linkConfirmacao + @"' class='btn-confirmar'>Confirmar Presença</a>
            </div>
        </div>
        
        <div class='convite-footer'>
            <p>Atenciosamente,<br>Equipe de organização do evento</p>
        </div>
    </div>
</body>
</html>";

            return html;
        }
    }
}