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
                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id;

                var resposta = await _eventBotService.ProcessarPerguntaAsync(pergunta, eventoId, userId);

                return Json(new { sucesso = true, resposta });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, resposta = $"Erro ao processar pergunta: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GerarSugestao(int eventoId)
        {
            try
            {
                var sugestao = await _eventBotService.GerarSugestaoEventoAsync(eventoId);
                return Json(new { sucesso = true, sugestao });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao gerar sugestão: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AnalisarOrcamento(int eventoId)
        {
            try
            {
                var analise = await _eventBotService.AnalisarOrcamentoAsync(eventoId);
                return Json(new { sucesso = true, analise });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao analisar orçamento: {ex.Message}" });
            }
        }
    }
}

