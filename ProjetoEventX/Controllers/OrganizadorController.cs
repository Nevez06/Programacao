using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class OrganizadorController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public OrganizadorController(EventXContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        // GET: Organizador
        public async Task<IActionResult> Index()
        {
            var eventXContext = _context.Organizadores.Include(o => o.Pessoa);
            return View(await eventXContext.ToListAsync());
        }

        // GET: Organizador/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var organizador = await _context.Organizadores
                .Include(o => o.Pessoa)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (organizador == null)
                return NotFound();

            return View(organizador);
        }

        // DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var organizador = await _context.Organizadores
                .Include(o => o.Pessoa)
                .Include(o => o.Eventos)
                    .ThenInclude(e => e.ListasConvidados)
                .Include(o => o.Eventos)
                    .ThenInclude(e => e.Despesas)
                .Include(o => o.Eventos)
                    .ThenInclude(e => e.TarefasEventos)
                .Include(o => o.Eventos)
                    .ThenInclude(e => e.ChecklistEventos)
                .Include(o => o.Eventos)
                    .ThenInclude(e => e.Pedidos)
                .FirstOrDefaultAsync(o => o.Email == user.Email);

            if (organizador == null)
                return NotFound();

            // Estatísticas para o dashboard
            var eventos = organizador.Eventos.ToList();
            ViewBag.TotalEventos = eventos.Count;
            ViewBag.EventosAtivos = eventos.Count(e => e.StatusEvento == "Planejado" || e.StatusEvento == "Em andamento");
            ViewBag.EventosConcluidos = eventos.Count(e => e.StatusEvento == "Concluído" || e.StatusEvento == "Finalizado");
            ViewBag.TotalConvidados = eventos.SelectMany(e => e.ListasConvidados).Count();
            ViewBag.ConvidadosConfirmados = eventos.SelectMany(e => e.ListasConvidados).Count(c => c.ConfirmaPresenca == "Confirmado");
            ViewBag.ConvidadosPendentes = eventos.SelectMany(e => e.ListasConvidados).Count(c => c.ConfirmaPresenca == "Pendente");
            ViewBag.ConvidadosRecusados = eventos.SelectMany(e => e.ListasConvidados).Count(c => c.ConfirmaPresenca == "Recusado" || c.ConfirmaPresenca == "Não irá");
            ViewBag.TotalDespesas = eventos.SelectMany(e => e.Despesas).Sum(d => d.Valor);
            ViewBag.TotalOrcamento = eventos.Sum(e => e.CustoEstimado);
            ViewBag.ProximoEvento = eventos
                .Where(e => e.DataEvento >= DateTime.Now)
                .OrderBy(e => e.DataEvento)
                .FirstOrDefault();

            // Dados para gráficos
            // Gráfico de gastos por evento (top 5)
            var gastosPorEvento = eventos
                .Where(e => e.Despesas.Any())
                .OrderByDescending(e => e.Despesas.Sum(d => d.Valor))
                .Take(5)
                .Select(e => new { Nome = e.NomeEvento, Valor = e.Despesas.Sum(d => d.Valor) })
                .ToList();
            ViewBag.GastosEventoNomes = System.Text.Json.JsonSerializer.Serialize(gastosPorEvento.Select(g => g.Nome));
            ViewBag.GastosEventoValores = System.Text.Json.JsonSerializer.Serialize(gastosPorEvento.Select(g => g.Valor));

            // Gráfico de status de convidados
            var todosConvidados = eventos.SelectMany(e => e.ListasConvidados).ToList();
            ViewBag.ConvidadosStatusLabels = System.Text.Json.JsonSerializer.Serialize(new[] { "Confirmados", "Pendentes", "Não irá" });
            ViewBag.ConvidadosStatusValores = System.Text.Json.JsonSerializer.Serialize(new[] {
                todosConvidados.Count(c => c.ConfirmaPresenca == "Confirmado"),
                todosConvidados.Count(c => c.ConfirmaPresenca == "Pendente"),
                todosConvidados.Count(c => c.ConfirmaPresenca == "Recusado" || c.ConfirmaPresenca == "Não irá")
            });

            // Gráfico de progresso de tarefas
            var todasTarefas = eventos.SelectMany(e => e.ChecklistEventos).ToList();
            var tarefasConcluidas = todasTarefas.Count(t => t.Concluido);
            var tarefasPendentes = todasTarefas.Count(t => !t.Concluido);
            ViewBag.TarefasConcluidas = tarefasConcluidas;
            ViewBag.TarefasPendentes = tarefasPendentes;
            ViewBag.TotalTarefas = todasTarefas.Count;

            // Gráfico de pedidos por status
            var todosPedidos = eventos.SelectMany(e => e.Pedidos).ToList();
            ViewBag.PedidosPendentes = todosPedidos.Count(p => p.StatusPedido == "Pendente");
            ViewBag.PedidosPagos = todosPedidos.Count(p => p.StatusPedido == "Pago");
            ViewBag.PedidosEntregues = todosPedidos.Count(p => p.StatusPedido == "Entregue");
            ViewBag.TotalPedidos = todosPedidos.Count;

            // Gastos mensais (últimos 6 meses)
            var despesasMensais = eventos.SelectMany(e => e.Despesas)
                .Where(d => d.DataDespesa >= DateTime.Now.AddMonths(-6))
                .GroupBy(d => new { d.DataDespesa.Year, d.DataDespesa.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { Mes = $"{g.Key.Month:D2}/{g.Key.Year}", Valor = g.Sum(d => d.Valor) })
                .ToList();
            ViewBag.DespesasMensaisLabels = System.Text.Json.JsonSerializer.Serialize(despesasMensais.Select(d => d.Mes));
            ViewBag.DespesasMensaisValores = System.Text.Json.JsonSerializer.Serialize(despesasMensais.Select(d => d.Valor));

            // Notificações recentes
            ViewBag.Notificacoes = await _context.Notificacoes
                .Where(n => n.DestinatarioId == organizador.PessoaId)
                .OrderByDescending(n => n.DataEnvio)
                .Take(5)
                .ToListAsync();
            ViewBag.NotificacoesNaoLidas = await _context.Notificacoes
                .CountAsync(n => n.DestinatarioId == organizador.PessoaId && !n.Lida);

            return View(organizador);
        }

        // GET: Organizador/Create
        public IActionResult Create()
        {
            ViewData["PessoaId"] = new SelectList(_context.Pessoas, "Id", "Email");
            return View();
        }

        // POST: Organizador/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PessoaId,DataCadastro,CreatedAt,UpdatedAt,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Organizador organizador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(organizador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["PessoaId"] = new SelectList(_context.Pessoas, "Id", "Email", organizador.PessoaId);
            return View(organizador);
        }

        // GET: Organizador/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var organizador = await _context.Organizadores.FindAsync(id);
            if (organizador == null)
                return NotFound();

            ViewData["PessoaId"] = new SelectList(_context.Pessoas, "Id", "Email", organizador.PessoaId);
            return View(organizador);
        }

        // POST: Organizador/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PessoaId,DataCadastro,CreatedAt,UpdatedAt,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Organizador organizador)
        {
            if (id != organizador.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(organizador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizadorExists(organizador.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["PessoaId"] = new SelectList(_context.Pessoas, "Id", "Email", organizador.PessoaId);
            return View(organizador);
        }

        // GET: Organizador/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var organizador = await _context.Organizadores
                .Include(o => o.Pessoa)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (organizador == null)
                return NotFound();

            return View(organizador);
        }

        // POST: Organizador/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var organizador = await _context.Organizadores.FindAsync(id);
            if (organizador != null)
            {
                _context.Organizadores.Remove(organizador);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Criar evento
        [HttpGet]
        public IActionResult CriarEvento()
        {
            ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"] ?? "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarEvento(Evento evento, string? CustoEstimadoTexto, string? LocalNome, string? LocalEndereco)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var organizador = await _context.Organizadores.FirstOrDefaultAsync(o => o.Email == user.Email);
            if (organizador == null)
                return NotFound();

            // OrganizadorId é definido pelo controller, não pelo form
            ModelState.Remove("OrganizadorId");
            ModelState.Remove("CustoEstimadoTexto");
            ModelState.Remove("LocalNome");
            ModelState.Remove("LocalEndereco");

            // Parsear custo estimado manualmente (suporte a vírgula)
            if (!string.IsNullOrWhiteSpace(CustoEstimadoTexto))
            {
                var valorStr = CustoEstimadoTexto.Replace(",", ".");
                if (decimal.TryParse(valorStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var custo))
                {
                    evento.CustoEstimado = custo;
                }
            }

            // Criar Local se informado
            if (!string.IsNullOrWhiteSpace(LocalEndereco))
            {
                var local = new Local
                {
                    NomeLocal = !string.IsNullOrWhiteSpace(LocalNome) ? LocalNome : "Local do Evento",
                    EnderecoLocal = LocalEndereco,
                    TipoLocal = "Evento",
                    Capacidade = evento.PublicoEstimado > 0 ? evento.PublicoEstimado : 100,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Locais.Add(local);
                await _context.SaveChangesAsync();
                evento.LocalId = local.Id;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"] ?? "";
                return View(evento);
            }

            evento.OrganizadorId = organizador.Id;
            evento.CreatedAt = DateTime.Now;
            evento.UpdatedAt = DateTime.Now;

            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> MeusEventos()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var eventos = await _context.Eventos
                .Where(e => e.OrganizadorId == user.Id)
                .OrderByDescending(e => e.DataEvento)
                .ToListAsync();

            return View(eventos);
        }

        [HttpGet]
        public async Task<IActionResult> DetalhesEvento(int id)
        {
            var evento = await _context.Eventos
                .Include(e => e.Pedidos)
                .Include(e => e.ListasConvidados)
                .ThenInclude(l => l.Convidado)
                .ThenInclude(c => c.Pessoa)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null)
                return NotFound();

            return View(evento);
        }

        // GET: Organizador/Orcamento
        public async Task<IActionResult> Orcamento()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var organizador = await _context.Organizadores
                .Include(o => o.Eventos)
                    .ThenInclude(e => e.Despesas)
                .FirstOrDefaultAsync(o => o.Email == user.Email);

            if (organizador == null)
                return NotFound();

            var eventos = organizador.Eventos.ToList();

            // Dados consolidados
            var totalOrcamentoEstimado = eventos.Sum(e => e.CustoEstimado);
            var totalGasto = eventos.SelectMany(e => e.Despesas).Sum(d => d.Valor);
            var saldo = totalOrcamentoEstimado - totalGasto;

            // Dados por evento
            var orcamentoPorEvento = eventos.Select(e => new OrcamentoEventoViewModel
            {
                EventoId = e.Id,
                NomeEvento = e.NomeEvento,
                DataEvento = e.DataEvento,
                TipoEvento = e.TipoEvento,
                StatusEvento = e.StatusEvento ?? "Planejado",
                CustoEstimado = e.CustoEstimado,
                TotalGasto = e.Despesas.Sum(d => d.Valor),
                Saldo = e.CustoEstimado - e.Despesas.Sum(d => d.Valor),
                QuantidadeDespesas = e.Despesas.Count,
                Despesas = e.Despesas.OrderByDescending(d => d.DataDespesa).ToList()
            }).OrderByDescending(o => o.DataEvento).ToList();

            // Despesas recentes (últimas 10 de todos os eventos)
            var despesasRecentes = eventos
                .SelectMany(e => e.Despesas.Select(d => new { Despesa = d, NomeEvento = e.NomeEvento }))
                .OrderByDescending(x => x.Despesa.DataDespesa)
                .Take(10)
                .ToList();

            ViewBag.TotalOrcamentoEstimado = totalOrcamentoEstimado;
            ViewBag.TotalGasto = totalGasto;
            ViewBag.Saldo = saldo;
            ViewBag.TotalEventos = eventos.Count;
            ViewBag.OrcamentoPorEvento = orcamentoPorEvento;
            ViewBag.DespesasRecentes = despesasRecentes
                .Select(x => new DespesaRecenteViewModel
                {
                    Descricao = x.Despesa.Descricao,
                    Valor = x.Despesa.Valor,
                    DataDespesa = x.Despesa.DataDespesa,
                    NomeEvento = x.NomeEvento
                }).ToList();

            return View();
        }

        // GET: Organizador/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var organizador = await _context.Organizadores
                .Include(o => o.Pessoa)
                .Include(o => o.Eventos)
                    .ThenInclude(e => e.Despesas)
                .Include(o => o.Eventos)
                    .ThenInclude(e => e.ListasConvidados)
                .FirstOrDefaultAsync(o => o.Email == user.Email);

            if (organizador == null)
                return NotFound();

            var eventos = organizador.Eventos.ToList();
            ViewBag.TotalEventos = eventos.Count;
            ViewBag.EventosAtivos = eventos.Count(e => e.StatusEvento == "Planejado" || e.StatusEvento == "Em andamento");
            ViewBag.EventosConcluidos = eventos.Count(e => e.StatusEvento == "Concluído" || e.StatusEvento == "Finalizado");
            ViewBag.TotalConvidados = eventos.SelectMany(e => e.ListasConvidados).Count();
            ViewBag.TotalGasto = eventos.SelectMany(e => e.Despesas).Sum(d => d.Valor);

            return View(organizador);
        }

        // POST: Organizador/Profile (Editar perfil)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string nome, string telefone, string endereco, string cidade, string uf)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var organizador = await _context.Organizadores
                .Include(o => o.Pessoa)
                .FirstOrDefaultAsync(o => o.Email == user.Email);

            if (organizador == null)
                return NotFound();

            // Atualizar dados da pessoa
            if (organizador.Pessoa != null)
            {
                organizador.Pessoa.Nome = nome ?? organizador.Pessoa.Nome;
                organizador.Pessoa.Telefone = telefone ?? organizador.Pessoa.Telefone;
                organizador.Pessoa.Endereco = endereco ?? organizador.Pessoa.Endereco;
                organizador.Pessoa.Cidade = cidade ?? organizador.Pessoa.Cidade;
                organizador.Pessoa.UF = uf ?? organizador.Pessoa.UF;
                organizador.Pessoa.UpdatedAt = DateTime.Now;
            }

            organizador.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Perfil atualizado com sucesso!";
            return RedirectToAction("Profile");
        }

        // POST: Organizador/AlterarSenha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(string senhaAtual, string novaSenha, string confirmarSenha)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("LoginOrganizador", "Auth");

            if (novaSenha != confirmarSenha)
            {
                TempData["Erro"] = "A nova senha e a confirmação não coincidem.";
                return RedirectToAction("Profile");
            }

            var result = await _userManager.ChangePasswordAsync(user, senhaAtual, novaSenha);
            if (result.Succeeded)
            {
                TempData["Sucesso"] = "Senha alterada com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao alterar senha: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Profile");
        }

        private bool OrganizadorExists(int id)
        {
            return _context.Organizadores.Any(e => e.Id == id);
        }
    }
}
