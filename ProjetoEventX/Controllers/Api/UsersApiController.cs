using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Users;
using ProjetoEventX.Models;
using ProjetoEventX.Security;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UsersApiController : ControllerBase
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersApiController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMe()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var profile = await BuildProfileDtoAsync(user, asNoTracking: true);
            return Ok(profile);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserProfileDto>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID inválido." });
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            var profile = await BuildProfileDtoAsync(user, asNoTracking: true);
            return Ok(profile);
        }

        [HttpPut("me")]
        public async Task<ActionResult<UserProfileDto>> UpdateMe([FromBody] UpdateUserProfileDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var nomeSanitizado = SanitizeIfProvided(request.Nome);
            var fotoUrlSanitizada = SanitizeIfProvided(request.FotoUrl);
            var telefoneSanitizado = SanitizeIfProvided(request.Telefone);
            var cidadeSanitizada = SanitizeIfProvided(request.Cidade);
            var enderecoSanitizado = SanitizeIfProvided(request.Endereco);
            var estadoNormalizado = NormalizeUfIfProvided(request.Estado);
            var cpfNormalizado = NormalizeCpfIfProvided(request.Cpf);

            if (!string.IsNullOrWhiteSpace(request.Nome) && !SecurityValidator.IsValidInput(request.Nome))
            {
                return BadRequest(new { message = "Nome inválido." });
            }

            if (!string.IsNullOrWhiteSpace(request.FotoUrl) && !SecurityValidator.IsValidInput(request.FotoUrl))
            {
                return BadRequest(new { message = "FotoUrl inválida." });
            }

            var updateApplied = await ApplyUserProfileUpdatesAsync(
                user,
                nomeSanitizado,
                fotoUrlSanitizada,
                telefoneSanitizado,
                cidadeSanitizada,
                estadoNormalizado,
                enderecoSanitizado,
                cpfNormalizado);
            if (!updateApplied)
            {
                return NotFound(new { message = "Perfil do usuário não encontrado." });
            }

            await _context.SaveChangesAsync();

            var updatedProfile = await BuildProfileDtoAsync(user, asNoTracking: true);
            return Ok(updatedProfile);
        }

        private async Task<bool> ApplyUserProfileUpdatesAsync(
            ApplicationUser user,
            string? nome,
            string? fotoUrl,
            string? telefone,
            string? cidade,
            string? estado,
            string? endereco,
            string? cpf)
        {
            switch (user.TipoUsuario)
            {
                case "Organizador":
                    var organizador = await _context.Organizadores
                        .Include(o => o.Pessoa)
                        .FirstOrDefaultAsync(o => o.Id == user.Id || o.Email == user.Email);

                    if (organizador?.Pessoa == null)
                    {
                        return false;
                    }

                    ApplyPessoaUpdate(organizador.Pessoa, nome, fotoUrl, telefone, cidade, estado, endereco, cpf);
                    organizador.UpdatedAt = DateTime.UtcNow;
                    return true;

                case "Fornecedor":
                    var fornecedor = await _context.Fornecedores
                        .Include(f => f.Pessoa)
                        .FirstOrDefaultAsync(f => f.Id == user.Id || f.Email == user.Email);

                    if (fornecedor?.Pessoa == null)
                    {
                        return false;
                    }

                    ApplyPessoaUpdate(fornecedor.Pessoa, nome, fotoUrl, telefone, cidade, estado, endereco, cpf);
                    if (!string.IsNullOrWhiteSpace(fotoUrl))
                    {
                        fornecedor.FotoPerfilUrl = fotoUrl;
                    }
                    fornecedor.UpdatedAt = DateTime.UtcNow;
                    return true;

                case "Convidado":
                    var convidado = await _context.Convidados
                        .Include(c => c.Pessoa)
                        .FirstOrDefaultAsync(c => c.Id == user.Id || c.Email == user.Email);

                    if (convidado?.Pessoa == null)
                    {
                        return false;
                    }

                    ApplyPessoaUpdate(convidado.Pessoa, nome, fotoUrl, telefone, cidade, estado, endereco, cpf);
                    convidado.UpdatedAt = DateTime.UtcNow;
                    return true;

                default:
                    var pessoa = await _context.Pessoas.FirstOrDefaultAsync(p => p.Email == user.Email);
                    if (pessoa == null)
                    {
                        return false;
                    }

                    ApplyPessoaUpdate(pessoa, nome, fotoUrl, telefone, cidade, estado, endereco, cpf);
                    return true;
            }
        }

        private async Task<UserProfileDto> BuildProfileDtoAsync(ApplicationUser user, bool asNoTracking)
        {
            var perfilDados = await ResolveProfileDataAsync(user, asNoTracking);

            var nome = perfilDados.Pessoa?.Nome;
            if (string.IsNullOrWhiteSpace(nome))
            {
                nome = user.UserName ?? user.Email ?? $"user-{user.Id}";
            }

            var fotoUrl = perfilDados.Pessoa?.FotoPerfilUrl;
            if (string.IsNullOrWhiteSpace(fotoUrl) && !string.IsNullOrWhiteSpace(perfilDados.FotoFornecedor))
            {
                fotoUrl = perfilDados.FotoFornecedor;
            }

            return new UserProfileDto
            {
                Id = user.Id,
                Nome = nome,
                Email = user.Email ?? string.Empty,
                FotoUrl = fotoUrl,
                TipoUsuario = user.TipoUsuario ?? string.Empty,
                Telefone = perfilDados.Pessoa?.Telefone ?? string.Empty,
                Cidade = perfilDados.Pessoa?.Cidade ?? string.Empty,
                Estado = perfilDados.Pessoa?.UF ?? string.Empty,
                Endereco = perfilDados.Pessoa?.Endereco ?? string.Empty,
                Cpf = perfilDados.Pessoa?.Cpf ?? string.Empty
            };
        }

        private async Task<(Pessoa? Pessoa, string? FotoFornecedor)> ResolveProfileDataAsync(ApplicationUser user, bool asNoTracking)
        {
            IQueryable<Organizador> queryOrganizador = _context.Organizadores.Include(o => o.Pessoa);
            IQueryable<Fornecedor> queryFornecedor = _context.Fornecedores.Include(f => f.Pessoa);
            IQueryable<Convidado> queryConvidado = _context.Convidados.Include(c => c.Pessoa);
            IQueryable<Pessoa> queryPessoa = _context.Pessoas;

            if (asNoTracking)
            {
                queryOrganizador = queryOrganizador.AsNoTracking();
                queryFornecedor = queryFornecedor.AsNoTracking();
                queryConvidado = queryConvidado.AsNoTracking();
                queryPessoa = queryPessoa.AsNoTracking();
            }

            switch (user.TipoUsuario)
            {
                case "Organizador":
                    var organizador = await queryOrganizador
                        .FirstOrDefaultAsync(o => o.Id == user.Id || o.Email == user.Email);
                    return (organizador?.Pessoa, null);

                case "Fornecedor":
                    var fornecedor = await queryFornecedor
                        .FirstOrDefaultAsync(f => f.Id == user.Id || f.Email == user.Email);
                    return (fornecedor?.Pessoa, fornecedor?.FotoPerfilUrl);

                case "Convidado":
                    var convidado = await queryConvidado
                        .FirstOrDefaultAsync(c => c.Id == user.Id || c.Email == user.Email);
                    return (convidado?.Pessoa, null);

                default:
                    var pessoa = await queryPessoa.FirstOrDefaultAsync(p => p.Email == user.Email);
                    return (pessoa, null);
            }
        }

        private static void ApplyPessoaUpdate(
            Pessoa pessoa,
            string? nome,
            string? fotoUrl,
            string? telefone,
            string? cidade,
            string? estado,
            string? endereco,
            string? cpf)
        {
            if (!string.IsNullOrWhiteSpace(nome))
            {
                pessoa.Nome = nome;
            }

            if (!string.IsNullOrWhiteSpace(fotoUrl))
            {
                pessoa.FotoPerfilUrl = fotoUrl;
            }

            if (!string.IsNullOrWhiteSpace(telefone))
            {
                pessoa.Telefone = telefone;
            }

            if (!string.IsNullOrWhiteSpace(cidade))
            {
                pessoa.Cidade = cidade;
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                pessoa.UF = estado;
            }

            if (!string.IsNullOrWhiteSpace(endereco))
            {
                pessoa.Endereco = endereco;
            }

            if (!string.IsNullOrWhiteSpace(cpf))
            {
                pessoa.Cpf = cpf;
            }

            pessoa.UpdatedAt = DateTime.UtcNow;
        }

        private static string? SanitizeIfProvided(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return SecurityValidator.SanitizeInput(value);
        }

        private static string? NormalizeUfIfProvided(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var sanitized = SecurityValidator.SanitizeInput(value).Trim().ToUpperInvariant();
            if (sanitized.Length >= 2)
            {
                return sanitized[..2];
            }

            return sanitized.PadRight(2, 'X');
        }

        private static string? NormalizeCpfIfProvided(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
            {
                return null;
            }

            var digits = new string(cpf.Where(char.IsDigit).ToArray());
            if (string.IsNullOrWhiteSpace(digits))
            {
                return null;
            }

            return digits.Length > 11 ? digits[..11] : digits;
        }
    }
}
