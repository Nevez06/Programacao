import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/invitations/data/models/invitation_draft_model.dart';
import 'package:projeto_eventx_flutter/features/invitations/data/models/invitation_template_model.dart';

class InvitationTemplatesRepository {
  InvitationTemplatesRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<InvitationTemplateModel>> getTemplates({int? eventId}) async {
    final list = await _apiClient.getList(
      ApiEndpoints.invitationTemplates,
      queryParameters: {
        if (eventId != null) 'eventId': eventId,
      },
    );
    final templates = <InvitationTemplateModel>[];
    for (final item in list) {
      if (item is Map<String, dynamic>) {
        templates.add(InvitationTemplateModel.fromJson(item));
        continue;
      }
      if (item is Map) {
        templates.add(
          InvitationTemplateModel.fromJson(
            Map<String, dynamic>.from(item),
          ),
        );
      }
    }
    return templates;
  }

  Future<List<InvitationDraftModel>> getDrafts({int? eventId}) async {
    final list = await _apiClient.getList(
      ApiEndpoints.invitationDrafts,
      queryParameters: {
        if (eventId != null) 'eventId': eventId,
      },
    );
    final drafts = <InvitationDraftModel>[];
    for (final item in list) {
      if (item is Map<String, dynamic>) {
        drafts.add(InvitationDraftModel.fromJson(item));
        continue;
      }
      if (item is Map) {
        drafts.add(
          InvitationDraftModel.fromJson(
            Map<String, dynamic>.from(item),
          ),
        );
      }
    }
    return drafts;
  }

  Future<InvitationDraftModel> saveDraft(
      SaveInvitationDraftRequest request) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.invitationDrafts,
      body: request.toJson(),
    );
    return InvitationDraftModel.fromJson(json);
  }

  Future<InvitationDraftModel> updateDraft(
    int draftId,
    SaveInvitationDraftRequest request,
  ) async {
    final json = await _apiClient.putJson(
      ApiEndpoints.invitationDraftById(draftId),
      body: request.toJson(),
    );
    return InvitationDraftModel.fromJson(json);
  }

  Future<void> deleteDraft(int draftId) async {
    await _apiClient.deleteVoid(ApiEndpoints.invitationDraftById(draftId));
  }

  Future<InvitationDraftModel> createFromTemplate(
    CreateInvitationFromTemplateRequest request,
  ) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.invitationCreateFromTemplate,
      body: request.toJson(),
    );
    return InvitationDraftModel.fromJson(json);
  }
}
