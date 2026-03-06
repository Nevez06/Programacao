using System.Net;
using System.Net.Mail;

namespace ProjetoEventX.Services
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromName;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.TryParse(configuration["Email:SmtpPort"], out var port) ? port : 587;
            _smtpUser = configuration["Email:SmtpUser"] ?? Environment.GetEnvironmentVariable("SMTP_USER") ?? "";
            _smtpPass = configuration["Email:SmtpPass"] ?? Environment.GetEnvironmentVariable("SMTP_PASS") ?? "";
            _fromName = configuration["Email:FromName"] ?? "EventX";
            _logger = logger;
        }

        public async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpoHtml)
        {
            if (string.IsNullOrEmpty(_smtpUser) || string.IsNullOrEmpty(_smtpPass))
            {
                _logger.LogWarning("SMTP não configurado. Configure SMTP_USER e SMTP_PASS nas variáveis de ambiente.");
                return false;
            }

            try
            {
                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_smtpUser, _fromName),
                    Subject = assunto,
                    Body = corpoHtml,
                    IsBodyHtml = true
                };
                message.To.Add(new MailAddress(destinatario));

                await client.SendMailAsync(message);
                _logger.LogInformation("Email enviado com sucesso para {Destinatario}", destinatario);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email para {Destinatario}", destinatario);
                return false;
            }
        }
    }
}
