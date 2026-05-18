using EventX.Api.Data;
using EventX.Api.DTOs.Invitations;
using EventX.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventX.Api.Services;

public sealed class InvitationService : IInvitationService
{
    private static readonly IReadOnlyList<SeedTemplate> DefaultTemplates = new List<SeedTemplate>
    {
        new("Classic Romance", "Romantico", "#FEE8E2", "#E35050", "#4A1F1F", "Playfair Display"),
        new("Corporate Elegance", "Corporativo", "#EDF4FF", "#2D7DF6", "#132647", "Inter"),
        new("Birthday Glow", "Aniversario", "#FFF4E5", "#FF8C3B", "#4D2B12", "Poppins"),
        new("Minimal Chic", "Minimalista", "#F7F7F7", "#111111", "#2C2C2C", "Montserrat")
    };

    private readonly AppDbContext _dbContext;

    public InvitationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InvitationTemplateDto>> GetTemplatesAsync(
        Guid organizerId,
        int? eventId,
        CancellationToken cancellationToken)
    {
        await EnsureDefaultTemplatesAsync(cancellationToken);

        var query = _dbContext.InvitationTemplates
            .AsNoTracking()
            .Where(x => x.DefaultSystem || x.OrganizerId == organizerId);

        if (eventId is > 0)
        {
            query = query.Where(x => x.EventId == null || x.EventId == eventId.Value);
        }

        var templates = await query
            .OrderByDescending(x => x.DefaultSystem)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return templates.Select(MapTemplate).ToList();
    }

    public async Task<IReadOnlyList<InvitationDraftDto>> GetDraftsAsync(
        Guid organizerId,
        int? eventId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.InvitationDrafts
            .AsNoTracking()
            .Where(x => x.OrganizerId == organizerId);

        if (eventId is > 0)
        {
            query = query.Where(x => x.EventId == eventId.Value);
        }

        var drafts = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return drafts.Select(MapDraft).ToList();
    }

    public async Task<InvitationDraftDto?> SaveDraftAsync(
        Guid organizerId,
        SaveInvitationDraftRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.EventId <= 0 || string.IsNullOrWhiteSpace(request.LayoutJson))
        {
            return null;
        }

        var eventOwned = await IsEventOwnedByOrganizerAsync(organizerId, request.EventId, cancellationToken);
        if (!eventOwned)
        {
            return null;
        }

        if (request.TemplateId is > 0)
        {
            var hasTemplate = await CanUseTemplateAsync(organizerId, request.TemplateId.Value, cancellationToken);
            if (!hasTemplate)
            {
                return null;
            }
        }

        InvitationDraft draft;
        if (request.Id is > 0)
        {
            draft = await _dbContext.InvitationDrafts
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id.Value && x.OrganizerId == organizerId,
                    cancellationToken);
            if (draft is null)
            {
                return null;
            }
        }
        else
        {
            draft = new InvitationDraft();
        }

        var isNew = draft.Id == 0;
        if (isNew)
        {
            draft.OrganizerId = organizerId;
            draft.CreatedAt = DateTime.UtcNow;
            _dbContext.InvitationDrafts.Add(draft);
        }

        draft.EventId = request.EventId;
        draft.TemplateId = request.TemplateId;
        draft.Name = ResolveDraftName(request.Name);
        draft.LayoutJson = request.LayoutJson.Trim();
        draft.PreviewHtml = Normalize(request.PreviewHtml);
        draft.PreviewUrl = Normalize(request.PreviewUrl);
        draft.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDraft(draft);
    }

    public async Task<InvitationDraftDto?> UpdateDraftAsync(
        Guid organizerId,
        int draftId,
        SaveInvitationDraftRequestDto request,
        CancellationToken cancellationToken)
    {
        if (draftId <= 0)
        {
            return null;
        }

        request.Id = draftId;
        return await SaveDraftAsync(organizerId, request, cancellationToken);
    }

    public async Task<bool> DeleteDraftAsync(
        Guid organizerId,
        int draftId,
        CancellationToken cancellationToken)
    {
        if (draftId <= 0)
        {
            return false;
        }

        var draft = await _dbContext.InvitationDrafts
            .FirstOrDefaultAsync(
                x => x.Id == draftId && x.OrganizerId == organizerId,
                cancellationToken);

        if (draft is null)
        {
            return false;
        }

        _dbContext.InvitationDrafts.Remove(draft);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<InvitationDraftDto?> CreateFromTemplateAsync(
        Guid organizerId,
        CreateInvitationFromTemplateRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.EventId <= 0 || request.TemplateId <= 0)
        {
            return null;
        }

        var eventEntity = await _dbContext.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.EventId && x.OrganizerId == organizerId,
                cancellationToken);

        if (eventEntity is null)
        {
            return null;
        }

        await EnsureDefaultTemplatesAsync(cancellationToken);

        var template = await _dbContext.InvitationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.TemplateId &&
                     (x.DefaultSystem || x.OrganizerId == organizerId),
                cancellationToken);

        if (template is null)
        {
            return null;
        }

        var draft = new InvitationDraft
        {
            EventId = eventEntity.Id,
            OrganizerId = organizerId,
            TemplateId = template.Id,
            Name = ResolveDraftName(request.Name, template.Name, eventEntity.Name),
            LayoutJson = BuildLayoutJson(template, eventEntity),
            PreviewUrl = template.PreviewUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.InvitationDrafts.Add(draft);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var invitation = new Invitation
        {
            EventId = eventEntity.Id,
            OrganizerId = organizerId,
            TemplateId = template.Id,
            DraftId = draft.Id,
            Name = draft.Name,
            LayoutJson = draft.LayoutJson,
            PreviewHtml = draft.PreviewHtml,
            PreviewUrl = draft.PreviewUrl,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Invitations.Add(invitation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapDraft(draft);
    }

    private async Task EnsureDefaultTemplatesAsync(CancellationToken cancellationToken)
    {
        var hasDefaultTemplates = await _dbContext.InvitationTemplates
            .AnyAsync(x => x.DefaultSystem, cancellationToken);

        if (hasDefaultTemplates)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var template in DefaultTemplates)
        {
            _dbContext.InvitationTemplates.Add(new InvitationTemplate
            {
                Name = template.Name,
                Style = template.Style,
                BackgroundColor = template.BackgroundColor,
                PrimaryColor = template.PrimaryColor,
                TextColor = template.TextColor,
                Font = template.Font,
                Title = $"{template.Name} Invitation",
                Message = "Customize seu convite no EventX Editor.",
                DefaultSystem = true,
                CreatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<bool> IsEventOwnedByOrganizerAsync(Guid organizerId, int eventId, CancellationToken cancellationToken)
    {
        return _dbContext.Events.AnyAsync(
            x => x.Id == eventId && x.OrganizerId == organizerId,
            cancellationToken);
    }

    private Task<bool> CanUseTemplateAsync(Guid organizerId, int templateId, CancellationToken cancellationToken)
    {
        return _dbContext.InvitationTemplates.AnyAsync(
            x => x.Id == templateId && (x.DefaultSystem || x.OrganizerId == organizerId),
            cancellationToken);
    }

    private static InvitationTemplateDto MapTemplate(InvitationTemplate template)
    {
        return new InvitationTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Style = template.Style,
            BackgroundColor = template.BackgroundColor,
            PrimaryColor = template.PrimaryColor,
            TextColor = template.TextColor,
            Font = template.Font,
            Title = template.Title,
            Message = template.Message,
            PreviewUrl = template.PreviewUrl,
            DefaultSystem = template.DefaultSystem,
            OrganizerId = template.OrganizerId,
            EventId = template.EventId
        };
    }

    private static InvitationDraftDto MapDraft(InvitationDraft draft)
    {
        return new InvitationDraftDto
        {
            Id = draft.Id,
            EventId = draft.EventId,
            OrganizerId = draft.OrganizerId,
            TemplateId = draft.TemplateId,
            Name = draft.Name,
            LayoutJson = draft.LayoutJson,
            PreviewHtml = draft.PreviewHtml,
            PreviewUrl = draft.PreviewUrl,
            UpdatedAt = draft.UpdatedAt
        };
    }

    private static string ResolveDraftName(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return $"Draft {DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string BuildLayoutJson(InvitationTemplate template, Event eventEntity)
    {
        var title = template.Title ?? eventEntity.Name;
        var message = template.Message ?? eventEntity.Description ?? string.Empty;
        var location = eventEntity.Location ?? string.Empty;
        var date = eventEntity.StartDate.ToString("yyyy-MM-ddTHH:mm:ssZ");

        return
            $$"""
            {"title":"{{EscapeJson(title)}}","message":"{{EscapeJson(message)}}","eventName":"{{EscapeJson(eventEntity.Name)}}","eventDate":"{{date}}","location":"{{EscapeJson(location)}}","style":"{{EscapeJson(template.Style ?? string.Empty)}}","backgroundColor":"{{EscapeJson(template.BackgroundColor ?? string.Empty)}}","primaryColor":"{{EscapeJson(template.PrimaryColor ?? string.Empty)}}","textColor":"{{EscapeJson(template.TextColor ?? string.Empty)}}","font":"{{EscapeJson(template.Font ?? string.Empty)}}"}
            """;
    }

    private static string EscapeJson(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }

    private sealed record SeedTemplate(
        string Name,
        string Style,
        string BackgroundColor,
        string PrimaryColor,
        string TextColor,
        string Font);
}
