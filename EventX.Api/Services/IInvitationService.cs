using EventX.Api.DTOs.Invitations;

namespace EventX.Api.Services;

public interface IInvitationService
{
    Task<IReadOnlyList<InvitationTemplateDto>> GetTemplatesAsync(
        Guid organizerId,
        int? eventId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<InvitationDraftDto>> GetDraftsAsync(
        Guid organizerId,
        int? eventId,
        CancellationToken cancellationToken);

    Task<InvitationDraftDto?> SaveDraftAsync(
        Guid organizerId,
        SaveInvitationDraftRequestDto request,
        CancellationToken cancellationToken);

    Task<InvitationDraftDto?> UpdateDraftAsync(
        Guid organizerId,
        int draftId,
        SaveInvitationDraftRequestDto request,
        CancellationToken cancellationToken);

    Task<bool> DeleteDraftAsync(
        Guid organizerId,
        int draftId,
        CancellationToken cancellationToken);

    Task<InvitationDraftDto?> CreateFromTemplateAsync(
        Guid organizerId,
        CreateInvitationFromTemplateRequestDto request,
        CancellationToken cancellationToken);
}
