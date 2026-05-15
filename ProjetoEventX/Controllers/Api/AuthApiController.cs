using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Auth;
using ProjetoEventX.Models;
using ProjetoEventX.Security;
using ProjetoEventX.Services;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private static readonly HashSet<string> TiposUsuarioValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "Organizador",
            "Fornecedor",
            "Convidado"
        };

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AuditoriaService _auditoriaService;
        private readonly JwtTokenService _jwtTokenService;
        private readonly EventXContext _context;

        public AuthApiController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AuditoriaService auditoriaService,
            JwtTokenService jwtTokenService,
            EventXContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditoriaService = auditoriaService;
            _jwtTokenService = jwtTokenService;
            _context = context;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var email = SecurityValidator.SanitizeInput(request.Email);
            if (!SecurityValidator.IsValidEmail(email))
            {
                return Unauthorized(new { message = "Credenciais inválidas." });
            }

            var tentativasRecentes = await _auditoriaService.ObterTentativasLoginRecentesAsync(email);
            if (tentativasRecentes >= 5)
            {
                await _auditoriaService.RegistrarLoginAsync(email, false, "Muitas tentativas de login.");
                return Unauthorized(new { message = "Muitas tentativas de login. Tente novamente em alguns minutos." });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                await _auditoriaService.RegistrarLoginAsync(email, false, "Usuário não encontrado.");
                return Unauthorized(new { message = "Credenciais inválidas." });
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                await _auditoriaService.RegistrarLoginAsync(email, false, "Email não confirmado.");
                return Unauthorized(new { message = "Credenciais inválidas." });
            }

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                var motivoFalha = result.IsLockedOut ? "Conta bloqueada." : "Senha inválida.";
                await _auditoriaService.RegistrarLoginAsync(email, false, motivoFalha);
                return Unauthorized(new { message = "Credenciais inválidas." });
            }

            await _auditoriaService.RegistrarLoginAsync(email, true);

            var response = new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email ?? email,
                UserName = user.UserName ?? email,
                TipoUsuario = user.TipoUsuario ?? string.Empty,
                Token = _jwtTokenService.GenerateToken(user)
            };

            return Ok(response);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var email = SecurityValidator.SanitizeInput(request.Email);
            if (!SecurityValidator.IsValidEmail(email))
            {
                return BadRequest(new { message = "Email inválido." });
            }

            var tipoUsuario = NormalizeTipoUsuario(request.TipoUsuario);
            if (!TiposUsuarioValidos.Contains(tipoUsuario))
            {
                return BadRequest(new { message = "Tipo de usuário inválido." });
            }

            var nome = SecurityValidator.SanitizeInput(request.NomeCompleto);
            if (string.IsNullOrWhiteSpace(nome))
            {
                return BadRequest(new { message = "Nome completo é obrigatório." });
            }

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                return Conflict(new { message = "Este email já está cadastrado." });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            ApplicationUser? user = null;

            try
            {
                user = new ApplicationUser
                {
                    UserName = BuildUserName(request.UserName, email),
                    Email = email,
                    TipoUsuario = tipoUsuario,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, request.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    return BadRequest(new { message = $"Não foi possível criar o usuário: {errors}" });
                }

                var pessoa = new Pessoa
                {
                    Nome = nome,
                    Email = email,
                    Endereco = string.IsNullOrWhiteSpace(request.Endereco)
                        ? "Endereco nao informado"
                        : SecurityValidator.SanitizeInput(request.Endereco),
                    Cpf = NormalizeCpf(request.Cpf),
                    Telefone = SanitizeOrDefault(request.Telefone),
                    Cidade = SanitizeOrDefault(request.Cidade),
                    UF = NormalizeUf(request.Uf),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Pessoas.Add(pessoa);
                await _context.SaveChangesAsync();

                switch (tipoUsuario)
                {
                    case "Organizador":
                        _context.Organizadores.Add(new Organizador
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            EmailConfirmed = true,
                            PessoaId = pessoa.Id,
                            Pessoa = pessoa,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                        break;

                    case "Fornecedor":
                        var cnpj = NormalizeCnpj(request.Cnpj);
                        if (string.IsNullOrWhiteSpace(cnpj))
                        {
                            return BadRequest(new { message = "CNPJ é obrigatório para fornecedor." });
                        }

                        _context.Fornecedores.Add(new Fornecedor
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            EmailConfirmed = true,
                            PessoaId = pessoa.Id,
                            Pessoa = pessoa,
                            Cnpj = cnpj,
                            TipoServico = string.IsNullOrWhiteSpace(request.TipoServico)
                                ? "Servicos gerais"
                                : SecurityValidator.SanitizeInput(request.TipoServico),
                            Cidade = string.IsNullOrWhiteSpace(request.Cidade)
                                ? "Nao informado"
                                : SecurityValidator.SanitizeInput(request.Cidade),
                            UF = NormalizeUf(request.Uf),
                            NomeNegocio = nome,
                            Descricao = string.Empty,
                            Categoria = "Geral",
                            FaixaPreco = "A definir",
                            ContatoComercial = email,
                            Disponibilidade = string.Empty,
                            FotoPerfilUrl = string.Empty,
                            Regiao = string.Empty,
                            Telefone = SanitizeOrDefault(request.Telefone),
                            ServicosOferecidos = new List<string>(),
                            Galeria = new List<string>(),
                            DataCadastro = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                        break;

                    default:
                        _context.Convidados.Add(new Convidado
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            EmailConfirmed = true,
                            PessoaId = pessoa.Id,
                            Pessoa = pessoa,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                        break;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditoriaService.RegistrarAcaoAsync(
                    "ApplicationUser",
                    user.Id,
                    "CREATE",
                    $"Cadastro via API: {tipoUsuario}",
                    null,
                    new { user.Id, user.Email, user.TipoUsuario });

                await _signInManager.SignInAsync(user, isPersistent: false);

                return Ok(new RegisterResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? email,
                    UserName = user.UserName ?? email,
                    TipoUsuario = user.TipoUsuario ?? tipoUsuario,
                    Token = _jwtTokenService.GenerateToken(user)
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Erro ao concluir o cadastro.",
                    detail = ex.Message
                });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<LoginResponseDto>> Me()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                TipoUsuario = user.TipoUsuario ?? string.Empty,
                Token = string.Empty
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return NoContent();
        }

        private static string NormalizeTipoUsuario(string? tipoUsuario)
        {
            var normalized = (tipoUsuario ?? string.Empty).Trim().ToLowerInvariant();
            return normalized switch
            {
                "organizador" => "Organizador",
                "fornecedor" => "Fornecedor",
                "convidado" => "Convidado",
                _ => tipoUsuario?.Trim() ?? string.Empty
            };
        }

        private static string BuildUserName(string? userName, string email)
        {
            var candidate = string.IsNullOrWhiteSpace(userName) ? email : userName;
            var sanitized = SecurityValidator.SanitizeInput(candidate);
            return string.IsNullOrWhiteSpace(sanitized) ? email : sanitized;
        }

        private static string NormalizeCpf(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
            {
                return "00000000000";
            }

            var digits = new string(cpf.Where(char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(digits) ? "00000000000" : digits;
        }

        private static string? NormalizeCnpj(string? cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
            {
                return null;
            }

            var digits = new string(cnpj.Where(char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(digits) ? null : digits;
        }

        private static string NormalizeUf(string? uf)
        {
            if (string.IsNullOrWhiteSpace(uf))
            {
                return "SP";
            }

            var sanitized = SecurityValidator.SanitizeInput(uf).ToUpperInvariant();
            if (sanitized.Length > 2)
            {
                return sanitized[..2];
            }

            return sanitized.PadRight(2, 'X');
        }

        private static string SanitizeOrDefault(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return SecurityValidator.SanitizeInput(value);
        }
    }
}
