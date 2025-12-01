using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using ProjetoEventX.Services;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class IAController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EventBotService _eventBotService;

        public IAController(EventXContext context, UserManager<ApplicationUser> userManager, EventBotService eventBotService)
        {
            _context = context;
            _userManager = userManager;
            _eventBotService = eventBotService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Verificar se o usuário é um organizador
            if (!await IsOrganizadorAsync())
            {
                TempData["ErrorMessage"] = "❌ Acesso negado! Apenas organizadores podem usar o assistente virtual.";
                TempData["AlertType"] = "error";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.UserName = User.Identity?.Name;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Perguntar(string pergunta, int? eventoId = null)
        {
            // Verificar se o usuário é um organizador
            if (!await IsOrganizadorAsync())
            {
                return Json(new { sucesso = false, resposta = "🚫 Acesso negado. Apenas organizadores podem usar esta funcionalidade." });
            }

            if (string.IsNullOrWhiteSpace(pergunta))
            {
                return Json(new { sucesso = false, resposta = "⚠️ Pergunta não pode estar vazia." });
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id;

                var resposta = await _eventBotService.ProcessarPerguntaAsync(pergunta, eventoId, userId);

                return Json(new { sucesso = true, resposta });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, resposta = $"❌ Erro ao processar pergunta: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GerarSugestao(int eventoId)
        {
            // Verificar se o usuário é um organizador
            if (!await IsOrganizadorAsync())
            {
                return Json(new { sucesso = false, mensagem = "🚫 Acesso negado. Apenas organizadores podem usar esta funcionalidade." });
            }

            try
            {
                var sugestao = await _eventBotService.GerarSugestaoEventoAsync(eventoId);
                return Json(new { sucesso = true, sugestao });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"❌ Erro ao gerar sugestão: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AnalisarOrcamento(int eventoId)
        {
            // Verificar se o usuário é um organizador
            if (!await IsOrganizadorAsync())
            {
                return Json(new { sucesso = false, mensagem = "🚫 Acesso negado. Apenas organizadores podem usar esta funcionalidade." });
            }

            try
            {
                var analise = await _eventBotService.AnalisarOrcamentoAsync(eventoId);
                return Json(new { sucesso = true, analise });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"❌ Erro ao analisar orçamento: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MeusEventos()
        {
            try
            {
                if (!await IsOrganizadorAsync())
                {
                    return Json(new { sucesso = false, mensagem = "Acesso negado." });
                }

                var user = await _userManager.GetUserAsync(User);
                var organizador = await _context.Organizadores
                    .FirstOrDefaultAsync(o => o.Email == user.Email);

                if (organizador == null)
                {
                    return Json(new { sucesso = false, eventos = new List<object>() });
                }

                var eventos = await _context.Eventos
                    .Where(e => e.OrganizadorId == organizador.Id)
                    .Select(e => new 
                    {
                        id = e.Id,
                        nome = e.NomeEvento,
                        data = e.DataEvento.ToString("dd/MM/yyyy"),
                        status = e.StatusEvento
                    })
                    .ToListAsync();

                return Json(new { sucesso = true, eventos });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        private async Task<bool> IsOrganizadorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return false;

            // Verificar se existe um organizador associado a este usuário
            var organizador = await _context.Organizadores
                .FirstOrDefaultAsync(o => o.Email == user.Email);

            return organizador != null;
        }
    }
}

