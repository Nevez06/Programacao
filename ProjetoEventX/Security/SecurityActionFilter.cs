using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoEventX.Security
{
    public class SecurityActionFilter : IAsyncActionFilter
    {
        private readonly EventXContext _context;

        public SecurityActionFilter(EventXContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Validação anti-injeção para parâmetros de entrada
            foreach (var argument in context.ActionArguments)
            {
                if (argument.Value is string strValue)
                {
                    if (ContainsDangerousInput(strValue))
                    {
                        context.Result = new BadRequestObjectResult("❌ Conteúdo suspeito detectado. Solicitação bloqueada por segurança.");
                        return;
                    }
                }
            }

            // Rate limiting simples (exemplo: máximo 100 requisições por IP por minuto)
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(remoteIp))
            {
                var now = DateTime.UtcNow;
                var oneMinuteAgo = now.AddMinutes(-1);
                
                // Verificar se há tentativas suspeitas
                var recentRequests = await _context.LogsAcessos
                    .Where(l => l.EnderecoIP == remoteIp && l.DataAcesso >= oneMinuteAgo)
                    .CountAsync();

                if (recentRequests > 100)
                {
                    context.Result = new StatusCodeResult(429); // Too Many Requests
                    return;
                }

                // Registrar acesso
                _context.LogsAcessos.Add(new LogsAcesso
                {
                    EnderecoIP = remoteIp,
                    DataAcesso = now,
                    Usuario = context.HttpContext.User.Identity.Name ?? "Anônimo",
                    UrlAcesso = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString.Value,
                    UserAgent = context.HttpContext.Request.Headers["User-Agent"].ToString()
                });

                await _context.SaveChangesAsync();
            }

            await next();
        }

        private bool ContainsDangerousInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var dangerousPatterns = new[]
            {
                "<script",
                "javascript:",
                "vbscript:",
                "onload=",
                "onerror=",
                "onclick=",
                "<iframe",
                "<object",
                "<embed",
                "<form",
                "' OR '",
                "' OR 1=1",
                "UNION SELECT",
                "DROP TABLE",
                "INSERT INTO",
                "DELETE FROM",
                "UPDATE ",
                "EXEC(",
                "xp_",
                "sp_",
                "../",
                "..\\",
                "%2e%2e",
                "0x3c",
                "0x3e"
            };

            var lowerInput = input.ToLowerInvariant();
            return dangerousPatterns.Any(pattern => lowerInput.Contains(pattern.ToLowerInvariant()));
        }
    }
}