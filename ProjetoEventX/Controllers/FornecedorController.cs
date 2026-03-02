using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
namespace ProjetoEventX.Controllers
{
    public class FornecedorController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FornecedorController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Fornecedor
        public async Task<IActionResult> Index()
        {
            // Se o usuário logado é Organizador, redireciona para o catálogo de produtos
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && user.TipoUsuario == "Organizador")
                {
                    return RedirectToAction(nameof(Catalogo));
                }
            }

            var eventXContext = _context.Fornecedores.Include(f => f.Pessoa);
            return View(await eventXContext.ToListAsync());
        }

        // GET: Fornecedor/Catalogo - Visualização de produtos para Organizadores
        public async Task<IActionResult> Catalogo()
        {
            var produtos = await _context.Produtos
                .Include(p => p.Fornecedor)
                    .ThenInclude(f => f.Pessoa)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            return View(produtos);
        }

        // GET: Fornecedor/DetalhesProduto/guid - Detalhes do produto com info do fornecedor
        public async Task<IActionResult> DetalhesProduto(Guid? id)
        {
            if (id == null)
                return NotFound();

            var produto = await _context.Produtos
                .Include(p => p.Fornecedor)
                    .ThenInclude(f => f.Pessoa)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
                return NotFound();

            return View(produto);
        }

        // GET: Fornecedor/Dashboard - Painel do Fornecedor
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Fornecedor")
                return RedirectToAction("LoginFornecedor", "Auth");

            var fornecedor = await _context.Fornecedores
                .Include(f => f.Pessoa)
                .Include(f => f.Produtos)
                .Include(f => f.Pedidos)
                .Include(f => f.Feedbacks)
                .FirstOrDefaultAsync(f => f.Email == user.Email);

            if (fornecedor == null)
                return NotFound();

            // Estatísticas para o dashboard
            ViewBag.TotalProdutos = fornecedor.Produtos.Count;
            ViewBag.TotalPedidos = fornecedor.Pedidos.Count;
            ViewBag.PedidosPendentes = fornecedor.Pedidos.Count(p => p.StatusPedido == "Pendente");
            ViewBag.ReceitaTotal = fornecedor.Pedidos.Where(p => p.StatusPedido == "Pago" || p.StatusPedido == "Entregue").Sum(p => p.PrecoTotal);
            ViewBag.TotalFeedbacks = fornecedor.Feedbacks.Count;
            ViewBag.AvaliacaoMedia = fornecedor.AvaliacaoMedia;

            return View(fornecedor);
        }

        // POST: Fornecedor/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(string ProductName, string ProductDescription, decimal ProductPrice, string ProductType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Fornecedor")
                return RedirectToAction("LoginFornecedor", "Auth");

            var fornecedor = await _context.Fornecedores.FirstOrDefaultAsync(f => f.Email == user.Email);
            if (fornecedor == null)
                return NotFound();

            var produto = new Produto
            {
                Nome = ProductName,
                Descricao = ProductDescription,
                Preco = ProductPrice,
                Tipo = ProductType ?? "Produto",
                FornecedorId = fornecedor.Id,
                Fornecedor = fornecedor
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }

        // GET: Fornecedor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fornecedor = await _context.Fornecedores
                .Include(f => f.Pessoa)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        // GET: Fornecedor/Create
        public IActionResult Create()
        {
            ViewData["PessoaId"] = new SelectList(_context.Pessoas, "Id", "Email");
            return View();
        }

        // POST: Fornecedor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PessoaId,Cnpj,TipoServico,AvaliacaoMedia,DataCadastro,CreatedAt,UpdatedAt,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Fornecedor fornecedor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fornecedor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PessoaId"] = new SelectList(_context.Pessoas, "Id", "Email", fornecedor.PessoaId);
            return View(fornecedor);
        }

        // GET: Fornecedor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fornecedor = await _context.Fornecedores.FindAsync(id);
            if (fornecedor == null)
            {
                return NotFound();
            }
            ViewData["PessoaId"] = new SelectList(_context.Pessoas, "Id", "Email", fornecedor.PessoaId);
            return View(fornecedor);
        }

        // POST: Fornecedor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PessoaId,Cnpj,TipoServico,AvaliacaoMedia,DataCadastro,CreatedAt,UpdatedAt,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Fornecedor fornecedor)
        {
            if (id != fornecedor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fornecedor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FornecedorExists(fornecedor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PessoaId"] = new SelectList(_context.Pessoas, "Id", "Email", fornecedor.PessoaId);
            return View(fornecedor);
        }

        // GET: Fornecedor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fornecedor = await _context.Fornecedores
                .Include(f => f.Pessoa)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        // POST: Fornecedor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fornecedor = await _context.Fornecedores.FindAsync(id);
            if (fornecedor != null)
            {
                _context.Fornecedores.Remove(fornecedor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FornecedorExists(int id)
        {
            return _context.Fornecedores.Any(e => e.Id == id);
        }
    }
}
