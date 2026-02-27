using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjetoEventX.Services
{
    public class AuditoriaService
    {
        private readonly EventXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditoriaService(EventXContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task RegistrarAcaoAsync(string tipoEntidade, int entidadeId, string tipoAcao, 
            string descricao, object? dadosAntigos = null, object? dadosNovos = null, bool sucesso = true, string? mensagemErro = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var userName = httpContext?.User?.Identity?.Name ?? "Sistema";
                var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Desconhecido";
                var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "";

                var auditoria = new Auditoria
                {
                    TipoEntidade = tipoEntidade,
                    EntidadeId = entidadeId,
                    TipoAcao = tipoAcao,
                    Usuario = userName,
                    DataAcao = DateTime.UtcNow,
                    EnderecoIP = ipAddress.Length > 45 ? ipAddress.Substring(0, 45) : ipAddress,
                    Descricao = descricao?.Length > 500 ? descricao.Substring(0, 500) : descricao,
                    Navegador = ObterNavegador(userAgent),
                    SistemaOperacional = ObterSistemaOperacional(userAgent),
                    Sucesso = sucesso,
                    MensagemErro = mensagemErro?.Length > 1000 ? mensagemErro.Substring(0, 1000) : mensagemErro
                };

                if (dadosAntigos != null)
                {
                    auditoria.DadosAntigos = JsonSerializer.Serialize(dadosAntigos, new JsonSerializerOptions { WriteIndented = true });
                }

                if (dadosNovos != null)
                {
                    auditoria.DadosNovos = JsonSerializer.Serialize(dadosNovos, new JsonSerializerOptions { WriteIndented = true });
                }

                _context.Auditorias.Add(auditoria);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Logar erro de auditoria (não deve falhar a operação principal)
                Console.WriteLine($"Erro ao registrar auditoria: {ex.Message}");
            }
        }

        public async Task RegistrarLoginAsync(string usuario, bool sucesso = true, string? mensagemErro = null)
        {
            await RegistrarAcaoAsync("ApplicationUser", 0, "LOGIN", "Tentativa de login", null, null, sucesso, mensagemErro);
        }

        public async Task RegistrarLogoutAsync(string usuario)
        {
            await RegistrarAcaoAsync("ApplicationUser", 0, "LOGOUT", "Logout do sistema");
        }

        public async Task RegistrarVisualizacaoAsync(string tipoEntidade, int entidadeId, string descricao)
        {
            await RegistrarAcaoAsync(tipoEntidade, entidadeId, "VIEW", descricao);
        }

        public async Task<int> ObterTentativasLoginRecentesAsync(string userName)
        {
            var vinteMinutosAtras = DateTime.UtcNow.AddMinutes(-20);
            return await _context.Auditorias
                .Where(a => a.Usuario == userName && 
                           a.TipoAcao == "LOGIN" && 
                           a.DataAcao >= vinteMinutosAtras && 
                           a.Sucesso == false)
                .CountAsync();
        }

        public async Task<bool> ExisteAtividadeSuspensaAsync(string ipAddress)
        {
            var umaHoraAtras = DateTime.UtcNow.AddHours(-1);
            var tentativasSuspensas = await _context.Auditorias
                .Where(a => a.EnderecoIP == ipAddress && 
                           a.DataAcao >= umaHoraAtras && 
                           (a.TipoAcao == "LOGIN" || a.TipoAcao == "CREATE" || a.TipoAcao == "UPDATE") &&
                           a.Sucesso == false)
                .CountAsync();

            return tentativasSuspensas > 10;
        }

        private static string ObterNavegador(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Desconhecido";

            if (userAgent.Contains("Chrome")) return "Chrome";
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) return "Safari";
            if (userAgent.Contains("Edge")) return "Edge";
            if (userAgent.Contains("Opera")) return "Opera";

            return "Outro";
        }

        private static string ObterSistemaOperacional(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Desconhecido";

            if (userAgent.Contains("Windows NT 10")) return "Windows 10";
            if (userAgent.Contains("Windows NT 6.3")) return "Windows 8.1";
            if (userAgent.Contains("Windows NT 6.2")) return "Windows 8";
            if (userAgent.Contains("Windows NT 6.1")) return "Windows 7";
            if (userAgent.Contains("Android")) return "Android";
            if (userAgent.Contains("iOS")) return "iOS";
            if (userAgent.Contains("Mac OS X")) return "macOS";
            if (userAgent.Contains("Linux")) return "Linux";

            return "Outro";
        }
    }
}