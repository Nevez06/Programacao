using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Invites;
using ProjetoEventX.Models;
using ProjetoEventX.Security;

namespace ProjetoEventX.Services
{
    public class InviteService
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly NotificationService _notificationService;

        public InviteService(
            EventXContext context,
            UserManager<ApplicationUser> userManager,
            EmailService emailService,
            NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync(System.Security.Claims.ClaimsPrincipal principal)
        {
            return await _userManager.GetUserAsync(principal);
        }

        public async Task<IReadOnlyList<InviteListDto>> GetInvitesAsync(ApplicationUser currentUser, int take = 200)
        {
            var normalizedTake = Math.Clamp(take, 1, 500);
            IQueryable<ListaConvidado> query = _context.ListasConvidados
                .AsNoTracking()
                .Include(i => i.Evento)
                .Include(i => i.Convidado)
                    .ThenInclude(c => c.Pessoa);

            if (IsOrganizador(currentUser))
            {
                query = query.Where(i => i.Evento != null && i.Evento.OrganizadorId == currentUser.Id);
            }
            else if (IsConvidado(currentUser))
            {
                query = query.Where(i => i.ConvidadoId == currentUser.Id);
            }
            else
            {
                return Array.Empty<InviteListDto>();
            }

            return await query
                .OrderByDescending(i => i.DataInclusao)
                .Take(normalizedTake)
                .Select(i => new InviteListDto
                {
                    Id = i.Id,
                    EventoId = i.EventoId,
                    EventoNome = i.Evento != null ? i.Evento.NomeEvento : "Evento",
                    ConvidadoId = i.ConvidadoId,
                    NomeConvidado = i.Convidado != null && i.Convidado.Pessoa != null ? i.Convidado.Pessoa.Nome : "Convidado",
                    EmailConvidado = i.Convidado != null && i.Convidado.Pessoa != null ? i.Convidado.Pessoa.Email : string.Empty,
                    Titulo = i.Evento != null ? $"Convite para {i.Evento.NomeEvento}" : "Convite",
                    Status = i.ConfirmaPresenca,
                    DataConvite = i.DataInclusao,
                    CheckInRealizado = i.CheckInRealizado,
                    CodigoQr = i.CodigoQR
                })
                .ToListAsync();
        }

        public async Task<InviteDetailsDto?> GetInviteDetailsAsync(ApplicationUser currentUser, int inviteId)
        {
            if (inviteId <= 0)
            {
                return null;
            }

            var invite = await _context.ListasConvidados
                .AsNoTracking()
                .Include(i => i.Evento)
                    .ThenInclude(e => e.Local)
                .Include(i => i.Convidado)
                    .ThenInclude(c => c.Pessoa)
                .FirstOrDefaultAsync(i => i.Id == inviteId);

            if (invite == null)
            {
                return null;
            }

            if (!CanAccessInvite(currentUser, invite))
            {
                throw new UnauthorizedAccessException("Sem permissao para visualizar este convite.");
            }

            var template = await _context.TemplatesConvites
                .AsNoTracking()
                .Where(t => t.EventoId == invite.EventoId && t.Ativo)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            return MapToDetailsDto(invite, template);
        }

        public async Task<InviteDetailsDto> CreateInviteAsync(ApplicationUser currentUser, CreateInviteDto request)
        {
            if (request.EventoId <= 0)
            {
                throw new InvalidOperationException("EventoId invalido.");
            }

            var evento = await _context.Eventos
                .Include(e => e.Local)
                .FirstOrDefaultAsync(e => e.Id == request.EventoId);

            if (evento == null)
            {
                throw new InvalidOperationException("Evento nao encontrado.");
            }

            if (!CanManageInvites(currentUser, evento))
            {
                throw new UnauthorizedAccessException("Sem permissao para gerenciar convites deste evento.");
            }

            var convidado = request.ConvidadoId.HasValue
                ? await _context.Convidados
                    .Include(c => c.Pessoa)
                    .FirstOrDefaultAsync(c => c.Id == request.ConvidadoId.Value)
                : await EnsureConvidadoAsync(request.NomeConvidado, request.EmailConvidado);

            if (convidado == null)
            {
                throw new InvalidOperationException("Convidado nao encontrado.");
            }

            var jaConvidado = await _context.ListasConvidados
                .AnyAsync(i => i.EventoId == request.EventoId && i.ConvidadoId == convidado.Id);

            if (jaConvidado)
            {
                throw new InvalidOperationException("Este convidado ja esta na lista do evento.");
            }

            var invite = new ListaConvidado
            {
                EventoId = request.EventoId,
                Evento = evento,
                ConvidadoId = convidado.Id,
                Convidado = convidado,
                DataInclusao = DateTime.UtcNow,
                ConfirmaPresenca = "Pendente",
                CodigoQR = $"EVTX-{request.EventoId}-{convidado.Id}-{Guid.NewGuid().ToString("N")[..8]}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ListasConvidados.Add(invite);
            await _context.SaveChangesAsync();

            var details = await GetInviteDetailsAsync(currentUser, invite.Id);
            return details ?? throw new InvalidOperationException("Nao foi possivel carregar o convite criado.");
        }

        public async Task<InviteDetailsDto?> UpdateInviteAsync(ApplicationUser currentUser, int inviteId, UpdateInviteDto request)
        {
            var invite = await _context.ListasConvidados
                .Include(i => i.Evento)
                .Include(i => i.Convidado)
                    .ThenInclude(c => c.Pessoa)
                .FirstOrDefaultAsync(i => i.Id == inviteId);

            if (invite == null)
            {
                return null;
            }

            if (!CanManageInvites(currentUser, invite.Evento))
            {
                throw new UnauthorizedAccessException("Sem permissao para atualizar este convite.");
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                invite.ConfirmaPresenca = NormalizeRsvpStatus(request.Status);
            }

            if (request.CheckInRealizado.HasValue)
            {
                invite.CheckInRealizado = request.CheckInRealizado.Value;
                invite.DataCheckIn = request.CheckInRealizado.Value ? DateTime.UtcNow : null;
            }

            invite.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetInviteDetailsAsync(currentUser, inviteId);
        }

        public async Task<InviteDetailsDto?> RespondRsvpAsync(ApplicationUser currentUser, int inviteId, RsvpResponseDto request)
        {
            var invite = await _context.ListasConvidados
                .Include(i => i.Evento)
                .Include(i => i.Convidado)
                    .ThenInclude(c => c.Pessoa)
                .FirstOrDefaultAsync(i => i.Id == inviteId);

            if (invite == null)
            {
                return null;
            }

            if (!CanRespondRsvp(currentUser, invite))
            {
                throw new UnauthorizedAccessException("Sem permissao para responder este convite.");
            }

            var novoStatus = NormalizeRsvpStatus(request.Resposta);
            var statusAnterior = invite.ConfirmaPresenca;
            invite.ConfirmaPresenca = novoStatus;
            invite.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (!string.Equals(statusAnterior, novoStatus, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(novoStatus, "Confirmado", StringComparison.OrdinalIgnoreCase) &&
                invite.Evento != null)
            {
                var nomeConvidado = invite.Convidado?.Pessoa?.Nome ?? "Um convidado";
                await _notificationService.CreateAsync(
                    invite.Evento.OrganizadorId,
                    "Presenca confirmada",
                    $"{nomeConvidado} confirmou presenca no evento '{invite.Evento.NomeEvento}'.",
                    "ConfirmacaoPresenca",
                    $"/Convite/Listar?eventoId={invite.EventoId}");
            }

            return await GetInviteDetailsAsync(currentUser, inviteId);
        }

        public async Task<bool?> SendInviteAsync(
            ApplicationUser currentUser,
            int inviteId,
            string? confirmationLink,
            string? mensagemOpcional)
        {
            var invite = await _context.ListasConvidados
                .Include(i => i.Evento)
                    .ThenInclude(e => e.Local)
                .Include(i => i.Convidado)
                    .ThenInclude(c => c.Pessoa)
                .FirstOrDefaultAsync(i => i.Id == inviteId);

            if (invite == null)
            {
                return null;
            }

            if (!CanManageInvites(currentUser, invite.Evento))
            {
                throw new UnauthorizedAccessException("Sem permissao para enviar este convite.");
            }

            var emailDestino = invite.Convidado?.Pessoa?.Email;
            if (string.IsNullOrWhiteSpace(emailDestino))
            {
                throw new InvalidOperationException("Convidado sem email valido para envio.");
            }

            var template = await _context.TemplatesConvites
                .AsNoTracking()
                .Where(t => t.EventoId == invite.EventoId && t.Ativo)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            var eventoNome = invite.Evento?.NomeEvento ?? "Evento";
            var nomeConvidado = invite.Convidado?.Pessoa?.Nome ?? "Convidado";
            var saudacao = template?.Saudacao ?? $"Ola {nomeConvidado},";
            var mensagemTemplate = template?.Mensagem ?? "Voce esta convidado!";
            var titulo = template?.Titulo ?? $"Convite para {eventoNome}";
            var complemento = string.IsNullOrWhiteSpace(mensagemOpcional) ? string.Empty : $"<p>{mensagemOpcional}</p>";
            var link = string.IsNullOrWhiteSpace(confirmationLink) ? string.Empty : confirmationLink!;

            var html = $@"
                <div style='font-family: Arial, sans-serif; max-width: 640px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h1>{titulo}</h1>
                    <p>{saudacao}</p>
                    <p>{mensagemTemplate}</p>
                    {complemento}
                    <p><strong>Evento:</strong> {eventoNome}</p>
                    <p><strong>Data:</strong> {invite.Evento?.DataEvento:dd/MM/yyyy}</p>
                    {(string.IsNullOrWhiteSpace(link) ? string.Empty : $"<p><a href='{link}' style='background-color:#2563eb;color:#fff;padding:12px 20px;text-decoration:none;border-radius:6px;display:inline-block;'>Responder convite</a></p>")}
                </div>";

            var emailEnviado = await _emailService.EnviarEmailAsync(emailDestino, $"Convite: {eventoNome}", html);

            invite.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return emailEnviado;
        }

        private async Task<Convidado?> EnsureConvidadoAsync(string? nomeConvidado, string? emailConvidado)
        {
            if (string.IsNullOrWhiteSpace(nomeConvidado) || string.IsNullOrWhiteSpace(emailConvidado))
            {
                throw new InvalidOperationException("Nome e email do convidado sao obrigatorios.");
            }

            if (!SecurityValidator.IsValidInput(nomeConvidado))
            {
                throw new InvalidOperationException("Nome do convidado invalido.");
            }

            if (!SecurityValidator.IsValidEmail(emailConvidado))
            {
                throw new InvalidOperationException("Email do convidado invalido.");
            }

            var nomeSanitizado = SecurityValidator.SanitizeInput(nomeConvidado);
            var emailSanitizado = SecurityValidator.SanitizeInput(emailConvidado);

            var pessoa = await _context.Pessoas.FirstOrDefaultAsync(p => p.Email == emailSanitizado);
            if (pessoa == null)
            {
                pessoa = new Pessoa
                {
                    Nome = nomeSanitizado,
                    Email = emailSanitizado,
                    Cpf = "00000000000",
                    Endereco = "Endereco nao informado",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Pessoas.Add(pessoa);
                await _context.SaveChangesAsync();
            }

            var convidado = await _context.Convidados
                .Include(c => c.Pessoa)
                .FirstOrDefaultAsync(c => c.PessoaId == pessoa.Id || c.Email == emailSanitizado);

            if (convidado != null)
            {
                return convidado;
            }

            var appUser = await _userManager.FindByEmailAsync(emailSanitizado);
            if (appUser == null)
            {
                appUser = new ApplicationUser
                {
                    UserName = emailSanitizado,
                    Email = emailSanitizado,
                    TipoUsuario = "Convidado",
                    EmailConfirmed = true
                };

                var password = $"{Guid.NewGuid().ToString("N")[..8]}@Aa1";
                var result = await _userManager.CreateAsync(appUser, password);
                if (!result.Succeeded)
                {
                    var erros = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Falha ao criar usuario do convidado: {erros}");
                }
            }
            else if (string.IsNullOrWhiteSpace(appUser.TipoUsuario))
            {
                appUser.TipoUsuario = "Convidado";
                await _userManager.UpdateAsync(appUser);
            }

            convidado = new Convidado
            {
                Id = appUser.Id,
                UserName = appUser.UserName ?? emailSanitizado,
                Email = appUser.Email ?? emailSanitizado,
                PessoaId = pessoa.Id,
                Pessoa = pessoa,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Convidados.Add(convidado);
            await _context.SaveChangesAsync();
            return convidado;
        }

        private static InviteListDto MapToListDto(ListaConvidado invite)
        {
            var eventoNome = invite.Evento?.NomeEvento ?? "Evento";
            return new InviteListDto
            {
                Id = invite.Id,
                EventoId = invite.EventoId,
                EventoNome = eventoNome,
                ConvidadoId = invite.ConvidadoId,
                NomeConvidado = invite.Convidado?.Pessoa?.Nome ?? "Convidado",
                EmailConvidado = invite.Convidado?.Pessoa?.Email ?? string.Empty,
                Titulo = $"Convite para {eventoNome}",
                Status = invite.ConfirmaPresenca,
                DataConvite = invite.DataInclusao,
                CheckInRealizado = invite.CheckInRealizado,
                CodigoQr = invite.CodigoQR
            };
        }

        private static InviteDetailsDto MapToDetailsDto(ListaConvidado invite, TemplateConvite? template)
        {
            var mapped = MapToListDto(invite);
            return new InviteDetailsDto
            {
                Id = mapped.Id,
                EventoId = mapped.EventoId,
                EventoNome = mapped.EventoNome,
                ConvidadoId = mapped.ConvidadoId,
                NomeConvidado = mapped.NomeConvidado,
                EmailConvidado = mapped.EmailConvidado,
                Titulo = mapped.Titulo,
                Status = mapped.Status,
                DataConvite = mapped.DataConvite,
                CheckInRealizado = mapped.CheckInRealizado,
                CodigoQr = mapped.CodigoQr,
                DataEvento = invite.Evento?.DataEvento,
                HoraInicio = invite.Evento?.HoraInicio,
                HoraFim = invite.Evento?.HoraFim,
                LocalNome = invite.Evento?.Local?.NomeLocal,
                LocalEndereco = invite.Evento?.Local?.EnderecoLocal,
                TemplateId = template?.Id,
                TemplateNome = template?.Nome
            };
        }

        private static bool IsOrganizador(ApplicationUser user)
        {
            return string.Equals(user.TipoUsuario, "Organizador", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsConvidado(ApplicationUser user)
        {
            return string.Equals(user.TipoUsuario, "Convidado", StringComparison.OrdinalIgnoreCase);
        }

        private static bool CanManageInvites(ApplicationUser user, Evento? evento)
        {
            return evento != null && IsOrganizador(user) && evento.OrganizadorId == user.Id;
        }

        private static bool CanRespondRsvp(ApplicationUser user, ListaConvidado invite)
        {
            if (CanManageInvites(user, invite.Evento))
            {
                return true;
            }

            return IsConvidado(user) && invite.ConvidadoId == user.Id;
        }

        private static bool CanAccessInvite(ApplicationUser user, ListaConvidado invite)
        {
            if (CanManageInvites(user, invite.Evento))
            {
                return true;
            }

            return IsConvidado(user) && invite.ConvidadoId == user.Id;
        }

        private static string NormalizeRsvpStatus(string? resposta)
        {
            var raw = resposta?.Trim();
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new InvalidOperationException("Resposta RSVP invalida.");
            }

            var normalized = raw.ToLowerInvariant().Replace("_", "").Replace(" ", "");
            return normalized switch
            {
                "confirmado" or "confirmar" or "sim" => "Confirmado",
                "naoira" or "naoir" or "nao" => "Nao ira",
                "recusado" => "Recusado",
                "pendente" => "Pendente",
                _ => throw new InvalidOperationException("Resposta RSVP nao suportada. Use Confirmado, NaoIra ou Recusado.")
            };
        }
    }
}
