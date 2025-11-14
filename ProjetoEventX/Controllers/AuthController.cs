using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EventXContext _context;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            EventXContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }



        // ------------------- REGISTRO ORGANIZADOR -------------------

        // GET: Exibir o formulário
        [HttpGet]
        public IActionResult RegistroOrganizador()
        {
            // Se houver mensagem no TempData (ex: sucesso), podemos passar para a ViewBag para mostrar na view
            if (TempData["MensagemSucesso"] != null)
            {
                ViewBag.MensagemSucesso = TempData["MensagemSucesso"];
            }
            if (TempData["MensagemErro"] != null)
            {
                ViewBag.MensagemErro = TempData["MensagemErro"];
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroOrganizador(RegistroOrganizadorViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    TipoUsuario = "Organizador"
                };

                var result = await _userManager.CreateAsync(user, model.Senha);
                if (result.Succeeded)
                {
                    // Cria pessoa vinculada ao organizador
                    var pessoa = new Pessoa
                    {
                        Nome = model.NomeCompleto,
                        Email = model.Email,
                        Endereco = model.Endereco,
                        Cpf = model.Cpf,
                        Telefone = model.Telefone
                    };

                    _context.Pessoas.Add(pessoa);
                    await _context.SaveChangesAsync();

                    // Cria organizador
                    var organizador = new Organizador
                    {
                        Id = user.Id, // 🔹 ADICIONE ISSO
                        PessoaId = pessoa.Id,
                        Pessoa = pessoa,
                        UserName = user.UserName,
                        Email = user.Email,
                        EmailConfirmed = true,

                    };

                    _context.Organizadores.Add(organizador);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Mensagem de sucesso? Como vai para Dashboard, melhor não usar aqui.
                    // Se quiser mostrar, faça no Dashboard (não recomendado geralmente).
                    return RedirectToAction("Dashboard", "Organizador");
                }

                // Adiciona erros para aparecer no ValidationSummary
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                // Não usar TempData para erro aqui
            }

            // Se ModelState inválido, retorna a view com os erros para o usuário corrigir.
            return View(model);
        }


        // ------------------- REGISTRO FORNECEDOR -------------------
        // GET: Exibir o formulário
        [HttpGet]
        public IActionResult RegistroFornecedor()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroFornecedor(RegistroFornecedorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    TipoUsuario = "Fornecedor"
                };

                var result = await _userManager.CreateAsync(user, model.Senha);
                if (result.Succeeded)
                {
                    // Cria pessoa vinculada ao fornecedor
                    var pessoa = new Pessoa
                    {
                        Nome = model.NomeLoja,
                        Email = model.Email,
                        Endereco = model.Endereco,
                        Cpf = model.Cpf,
                        Telefone = model.Telefone
                    };

                    _context.Pessoas.Add(pessoa);
                    await _context.SaveChangesAsync();

                    // Cria fornecedor
                    var fornecedor = new Fornecedor
                    {
                        PessoaId = pessoa.Id,
                        Pessoa = pessoa,
                        Cnpj = model.Cnpj,
                        TipoServico = model.TipoServico,
                        UserName = user.UserName,
                        Email = user.Email,
                        EmailConfirmed = true
                    };

                    _context.Fornecedores.Add(fornecedor);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Dashboard", "Fornecedor");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }




        // ------------------- LOGIN ORGANIZADOR -------------------

        [HttpGet]
        public IActionResult LoginOrganizador()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginOrganizador(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && user.TipoUsuario == "Organizador")
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Senha, false, false);
                    if (result.Succeeded)
                        return RedirectToAction("Dashboard", "Organizador");
                }

                ModelState.AddModelError("", "Credenciais inválidas para organizador.");
            }

            return View(model);
        }

        // ------------------- LOGIN CONVIDADO -------------------

        [HttpGet]
        public IActionResult LoginConvidado()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginConvidado(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && user.TipoUsuario == "Convidado")
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Senha, false, false);
                    if (result.Succeeded)
                        return RedirectToAction("Dashboard", "Convidado");
                }

                ModelState.AddModelError("", "Credenciais inválidas para convidado.");
            }

            return View(model);
        }

        // ------------------- LOGIN FORNECEDOR -------------------

        [HttpGet]
        public IActionResult LoginFornecedor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginFornecedor(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && user.TipoUsuario == "Fornecedor")
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Senha, false, false);
                    if (result.Succeeded)
                        return RedirectToAction("Dashboard", "Fornecedor");
                }

                ModelState.AddModelError("", "Credenciais inválidas para fornecedor.");
            }

            return View(model);
        }

        // ------------------- LOGOUT -------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }

    // ------------------- VIEWMODELS -------------------

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, ErrorMessage = "A {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar senha")]
        [Compare("Password", ErrorMessage = "A senha e a confirmação de senha não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Endereço")]
        public string Endereco { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Telefone")]
        public int Telefone { get; set; }

        [Required]
        [Display(Name = "CPF")]
        public string Cpf { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;
    }
}
