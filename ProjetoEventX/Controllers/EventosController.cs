using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ProjetoEventX.Data;
using ProjetoEventX.Models;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class EventosController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventosController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Eventos
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            // Buscar apenas eventos do organizador logado
            var eventos = await _context.Eventos
                .Where(e => e.OrganizadorId == user.Id)
                .OrderByDescending(e => e.DataEvento)
                .ToListAsync();
                
            return View(eventos);
        }

        // GET: Eventos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evento = await _context.Eventos
                .Include(e => e.Organizador)
                .ThenInclude(o => o.Pessoa)
                .Include(e => e.Local)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (evento == null)
            {
                return NotFound();
            }

            return View(evento);
        }

        // GET: Eventos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Eventos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NomeEvento,DescricaoEvento,DataEvento,TipoEvento,StatusEvento,HoraInicio,HoraFim,PublicoEstimado,CustoEstimado,LocalId")] Evento evento)
        {
            try
            {
                // Buscar o organizador logado
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "❌ Usuário não encontrado. Faça login novamente.";
                    return RedirectToAction("LoginOrganizador", "Auth");
                }

                // Buscar o organizador no banco de dados
                var organizador = await _context.Organizadores
                    .FirstOrDefaultAsync(o => o.Id == user.Id);

                if (organizador == null)
                {
                    TempData["ErrorMessage"] = "❌ Organizador não encontrado. Verifique seu cadastro.";
                    return RedirectToAction("Dashboard", "Organizador");
                }

                // Definir o OrganizadorId
                evento.OrganizadorId = organizador.Id;
                evento.CreatedAt = DateTime.UtcNow;
                evento.UpdatedAt = DateTime.UtcNow;

                // Validar ModelState após definir OrganizadorId
                ModelState.Remove("OrganizadorId"); // Remove validação do campo que será preenchido automaticamente
                
                if (ModelState.IsValid)
                {
                    _context.Add(evento);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "✅ Evento criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Erro ao criar evento: {ex.Message}";
            }

            return View(evento);
        }

        // GET: Eventos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null)
            {
                return NotFound();
            }

            // Verificar se o evento pertence ao usuário logado
            var user = await _userManager.GetUserAsync(User);
            if (user == null || evento.OrganizadorId != user.Id)
            {
                TempData["ErrorMessage"] = "❌ Você não tem permissão para editar este evento.";
                return RedirectToAction(nameof(Index));
            }

            return View(evento);
        }

        // POST: Eventos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NomeEvento,DescricaoEvento,Data Evento,TipoEvento,StatusEvento,HoraInicio,HoraFim,PublicoEstimado,CustoEstimado,LocalId")] Evento evento)
        {
            if (id != evento.Id)
            {
                return NotFound();
            }

            try
            {
                // Buscar o organizador logado
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("LoginOrganizador", "Auth");
                }

                // Buscar o evento original para manter o OrganizadorId
                var eventoOriginal = await _context.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                if (eventoOriginal == null || eventoOriginal.OrganizadorId != user.Id)
                {
                    TempData["ErrorMessage"] = "❌ Você não tem permissão para editar este evento.";
                    return RedirectToAction(nameof(Index));
                }

                // Manter dados originais importantes
                evento.OrganizadorId = eventoOriginal.OrganizadorId;
                evento.CreatedAt = eventoOriginal.CreatedAt;
                evento.UpdatedAt = DateTime.UtcNow;

                ModelState.Remove("OrganizadorId");

                if (ModelState.IsValid)
                {
                    _context.Update(evento);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "✅ Evento atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventoExists(evento.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Erro ao atualizar evento: {ex.Message}";
            }

            return View(evento);
        }

        // GET: Eventos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evento = await _context.Eventos
                .Include(e => e.Organizador)
                .ThenInclude(o => o.Pessoa)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (evento == null)
            {
                return NotFound();
            }

            // Verificar se o evento pertence ao usuário logado
            var user = await _userManager.GetUserAsync(User);
            if (user == null || evento.OrganizadorId != user.Id)
            {
                TempData["ErrorMessage"] = "❌ Você não tem permissão para excluir este evento.";
                return RedirectToAction(nameof(Index));
            }

            return View(evento);
        }

        // POST: Eventos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var evento = await _context.Eventos.FindAsync(id);
                if (evento != null)
                {
                    // Verificar se o evento pertence ao usuário logado
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null || evento.OrganizadorId != user.Id)
                    {
                        TempData["ErrorMessage"] = "❌ Você não tem permissão para excluir este evento.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Eventos.Remove(evento);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "✅ Evento excluído com sucesso!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Erro ao excluir evento: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventoExists(int id)
        {
            return _context.Eventos.Any(e => e.Id == id);
        }
    }
}
