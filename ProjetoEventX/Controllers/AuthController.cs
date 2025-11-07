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

        // ------------------- REGISTER -------------------

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    TipoUsuario = "Organizador"
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var pessoa = new Pessoa
                    {
                        Nome = model.Nome,
                        Email = model.Email,
                        Endereco = model.Endereco,
                        Telefone = model.Telefone,
                        Cpf = model.Cpf
                    };

                    _context.Pessoas.Add(pessoa);
                    await _context.SaveChangesAsync();

                    var organizador = new Organizador
                    {
                        Id = user.Id,
                        PessoaId = pessoa.Id,
                        Pessoa = pessoa,
                        UserName = user.UserName,
                        Email = user.Email,
                        EmailConfirmed = true
                    };

                    _context.Organizadores.Add(organizador);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
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
