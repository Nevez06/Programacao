using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using ProjetoEventX.Security;
using ProjetoEventX.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjetoEventX.Controllers
{
    [ServiceFilter(typeof(SecurityActionFilter))]
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EventXContext _context;
        private readonly AuditoriaService _auditoriaService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            EventXContext context,
            AuditoriaService auditoriaService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _auditoriaService = auditoriaService;
        }

        // ------------------- REGISTRO ORGANIZADOR -------------------
        [HttpGet]
        public IActionResult RegistroOrganizador()
        {
            if (TempData["MensagemSucesso"] != null)
                ViewBag.MensagemSucesso = TempData["MensagemSucesso"];
            if (TempData["MensagemErro"] != null)
                ViewBag.MensagemErro = TempData["MensagemErro"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroOrganizador(RegistroOrganizadorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Validações de segurança adicionais
            if (!SecurityValidator.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("", "❌ Email inválido ou suspeito.");
                return View(model);
            }

            if (!SecurityValidator.IsValidCPF(model.Cpf))
            {
                ModelState.AddModelError("", "❌ CPF inválido.");
                return View(model);
            }

            if (!SecurityValidator.IsValidInput(model.NomeCompleto))
            {
                ModelState.AddModelError("", "❌ Nome contém caracteres inválidos.");
                return View(model);
            }

            // Verificar se já existe usuário com este email
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "❌ Este email já está cadastrado.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = SecurityValidator.SanitizeInput(model.Email),
                Email = SecurityValidator.SanitizeInput(model.Email),
                TipoUsuario = "Organizador",
                EmailConfirmed = true // Confirma automaticamente no registro
            };

            var result = await _userManager.CreateAsync(user, model.Senha);
            if (result.Succeeded)
            {
                try
                {
                    // Criar pessoa com dados sanitizados
                    var pessoa = new Pessoa
                    {
                        Nome = SecurityValidator.SanitizeInput(model.NomeCompleto),
                        Email = SecurityValidator.SanitizeInput(model.Email),
                        Endereco = SecurityValidator.SanitizeInput(model.Endereco),
                        Cpf = SecurityValidator.SanitizeInput(Regex.Replace(model.Cpf, @"[^\d]", "")),
                        Telefone = SecurityValidator.SanitizeInput(model.Telefone),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Pessoas.Add(pessoa);
                    await _context.SaveChangesAsync();

                    // Criar organizador
                    var organizador = new Organizador
                    {
                        Id = user.Id,
                        PessoaId = pessoa.Id,
                        Pessoa = pessoa,
                        UserName = user.UserName,
                        Email = user.Email,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Organizadores.Add(organizador);
                    await _context.SaveChangesAsync();

                    // Registrar auditoria
                    await _auditoriaService.RegistrarAcaoAsync("ApplicationUser", user.Id, "CREATE", 
                        $"Novo organizador criado: {pessoa.Nome}", null, new { user.Id, pessoa.Nome, pessoa.Email });

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Dashboard", "Organizador");
                }
                catch (Exception ex)
                {
                    // Se houver erro, remover usuário criado
                    await _userManager.DeleteAsync(user);
                    ModelState.AddModelError("", "❌ Erro ao criar organizador. Tente novamente.");
                }
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // ------------------- REGISTRO FORNECEDOR -------------------
        [HttpGet]
        public IActionResult RegistroFornecedor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroFornecedor(RegistroFornecedorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Validações de segurança
            if (!SecurityValidator.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("", "❌ Email inválido ou suspeito.");
                return View(model);
            }

            if (!SecurityValidator.IsValidInput(model.NomeLoja))
            {
                ModelState.AddModelError("", "❌ Nome da loja contém caracteres inválidos.");
                return View(model);
            }

            // Validar CNPJ
            var cnpjLimpo = Regex.Replace(model.Cnpj, @"[^\d]", "");
            if (cnpjLimpo.Length != 14)
            {
                ModelState.AddModelError("", "❌ CNPJ inválido.");
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "❌ Este email já está cadastrado.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = SecurityValidator.SanitizeInput(model.Email),
                Email = SecurityValidator.SanitizeInput(model.Email),
                TipoUsuario = "Fornecedor",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Senha);
            if (result.Succeeded)
            {
                try
                {
                    var pessoa = new Pessoa
                    {
                        Nome = SecurityValidator.SanitizeInput(model.NomeLoja),
                        Email = SecurityValidator.SanitizeInput(model.Email),
                        Endereco = SecurityValidator.SanitizeInput(model.Endereco),
                        Cpf = SecurityValidator.SanitizeInput(Regex.Replace(model.Cpf, @"[^\d]", "")),
                        Telefone = SecurityValidator.SanitizeInput(model.Telefone),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Pessoas.Add(pessoa);
                    await _context.SaveChangesAsync();

                    var fornecedor = new Fornecedor
                    {
                        PessoaId = pessoa.Id,
                        Pessoa = pessoa,
                        Cnpj = SecurityValidator.SanitizeInput(cnpjLimpo),
                        TipoServico = SecurityValidator.SanitizeInput(model.TipoServico),
                        Cidade = SecurityValidator.SanitizeInput(model.Cidade),
                        UF = SecurityValidator.SanitizeInput(model.UF.ToUpper()),
                        UserName = user.UserName,
                        Email = user.Email,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Fornecedores.Add(fornecedor);
                    await _context.SaveChangesAsync();

                    await _auditoriaService.RegistrarAcaoAsync("ApplicationUser", user.Id, "CREATE",
                        $"Novo fornecedor criado: {pessoa.Nome}", null, new { user.Id, pessoa.Nome, pessoa.Email });

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Dashboard", "Fornecedor");
                }
                catch (Exception)
                {
                    await _userManager.DeleteAsync(user);
                    ModelState.AddModelError("", "❌ Erro ao criar fornecedor. Tente novamente.");
                }
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // ------------------- LOGIN ORGANIZADOR -------------------
        [HttpGet]
        public IActionResult LoginOrganizador()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Dashboard", "Organizador");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginOrganizador(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Verificar tentativas de login recentes
            var tentativasRecentes = await _auditoriaService.ObterTentativasLoginRecentesAsync(model.Email);
            if (tentativasRecentes >= 5)
            {
                ModelState.AddModelError("", "❌ Muitas tentativas de login falhadas. Aguarde 20 minutos.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && user.TipoUsuario == "Organizador")
            {
                // Verificar se a conta está ativa
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError("", "❌ Email não confirmado. Verifique sua caixa de entrada.");
                    await _auditoriaService.RegistrarLoginAsync(model.Email, false, "Email não confirmado");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Senha, false, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    await _auditoriaService.RegistrarLoginAsync(model.Email, true);
                    return RedirectToAction("Dashboard", "Organizador");
                }
                else
                {
                    await _auditoriaService.RegistrarLoginAsync(model.Email, false, "Senha incorreta");
                }
            }

            ModelState.AddModelError("", "❌ Credenciais inválidas para organizador.");
            return View(model);
        }

        // ------------------- LOGIN FORNECEDOR -------------------
        [HttpGet]
        public IActionResult LoginFornecedor()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Dashboard", "Fornecedor");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginFornecedor(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tentativasRecentes = await _auditoriaService.ObterTentativasLoginRecentesAsync(model.Email);
            if (tentativasRecentes >= 5)
            {
                ModelState.AddModelError("", "❌ Muitas tentativas de login falhadas. Aguarde 20 minutos.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && user.TipoUsuario == "Fornecedor")
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError("", "❌ Email não confirmado.");
                    await _auditoriaService.RegistrarLoginAsync(model.Email, false, "Email não confirmado");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Senha, false, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    await _auditoriaService.RegistrarLoginAsync(model.Email, true);
                    return RedirectToAction("Dashboard", "Fornecedor");
                }
                else
                {
                    await _auditoriaService.RegistrarLoginAsync(model.Email, false, "Senha incorreta");
                }
            }

            ModelState.AddModelError("", "❌ Credenciais inválidas para fornecedor.");
            return View(model);
        }

        // ------------------- LOGIN CONVIDADO -------------------
        [HttpGet]
        public IActionResult LoginConvidado()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Dashboard", "Convidado");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginConvidado(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tentativasRecentes = await _auditoriaService.ObterTentativasLoginRecentesAsync(model.Email);
            if (tentativasRecentes >= 5)
            {
                ModelState.AddModelError("", "❌ Muitas tentativas de login falhadas. Aguarde 20 minutos.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && user.TipoUsuario == "Convidado")
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError("", "❌ Email não confirmado.");
                    await _auditoriaService.RegistrarLoginAsync(model.Email, false, "Email não confirmado");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Senha, false, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    await _auditoriaService.RegistrarLoginAsync(model.Email, true);
                    return RedirectToAction("Dashboard", "Convidado");
                }
                else
                {
                    await _auditoriaService.RegistrarLoginAsync(model.Email, false, "Senha incorreta");
                }
            }

            ModelState.AddModelError("", "❌ Credenciais inválidas para convidado.");
            return View(model);
        }

        // ------------------- LOGOUT -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity.Name ?? "Anônimo";
            await _auditoriaService.RegistrarLogoutAsync(userName);
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ------------------- ACESSO NEGADO -------------------
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ------------------- VIEWMODELS -------------------
        public class LoginViewModel
        {
            [Required(ErrorMessage = "Email é obrigatório")]
            [EmailAddress(ErrorMessage = "Email inválido")]
            [StringLength(256, ErrorMessage = "Email muito longo")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Senha é obrigatória")]
            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 1, ErrorMessage = "Senha é obrigatória")]
            public string Senha { get; set; } = string.Empty;

            public bool LembrarMe { get; set; }
        }
    }
}