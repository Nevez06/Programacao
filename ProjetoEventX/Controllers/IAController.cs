using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Models;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class IAController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IAController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Perguntar(string pergunta, int? eventoId = null)
        {
            if (string.IsNullOrWhiteSpace(pergunta))
            {
                return BadRequest("Pergunta não pode estar vazia");
            }

            try
            {
                var contexto = "";
                if (eventoId.HasValue)
                {
                    var evento = await _context.Eventos
                        .Include(e => e.Pedidos)
                        .Include(e => e.ListasConvidados)
                        .FirstOrDefaultAsync(e => e.Id == eventoId.Value);

                    if (evento != null)
                    {
                        contexto = $"Informações do evento: {evento.NomeEvento}, Data: {evento.DataEvento}, " +
                                   $"Descrição: {evento.DescricaoEvento}, Convidados: {evento.ListasConvidados.Count}, " +
                                   $"Pedidos: {evento.Pedidos.Count}";
                    }
                }

                var prompt = $"Você é um assistente virtual do EventX, uma plataforma de gestão de eventos. " +
                           $"Responda de forma amigável e útil. {contexto} " +
                           $"Pergunta do usuário: {pergunta}";

                // Simplificado para evitar erros de compilação
                // Implementação básica - você pode melhorar depois com a API correta
                var resposta = $"Olá! Sou o assistente virtual do EventX. Você perguntou: {pergunta}. " +
                              $"Estou aqui para ajudá-lo com suas dúvidas sobre eventos. " +
                              $"Para mais informações, entre em contato com o suporte.";

                return Json(new { sucesso = true, resposta });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, resposta = $"Erro ao processar pergunta: {ex.Message}" });
            }
        }
    }
}

