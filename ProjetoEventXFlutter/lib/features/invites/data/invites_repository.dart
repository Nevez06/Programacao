import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/create_invite_model.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/invite_details_model.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/invite_model.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/rsvp_response_model.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/send_invite_result_model.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/update_invite_model.dart';

class InvitesRepository {
  InvitesRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  bool get supportsInvitesApi => true;

  Future<List<InviteModel>> getInvites() async {
    final list = await _apiClient.getList(ApiEndpoints.invites);
    return list
        .whereType<Map<String, dynamic>>()
        .map(InviteModel.fromJson)
        .toList();
  }

  Future<InviteDetailsModel> getInviteDetails(int inviteId) async {
    final json = await _apiClient.getJson(ApiEndpoints.inviteById(inviteId));
    return InviteDetailsModel.fromJson(json);
  }

  Future<CreateInviteResultModel> createInvite(CreateInviteModel model) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.invites,
      body: model.toJson(),
    );
    return CreateInviteResultModel.fromJson(json);
  }

  Future<InviteDetailsModel> updateInvite(
    int inviteId,
    UpdateInviteModel model,
  ) async {
    final json = await _apiClient.putJson(
      ApiEndpoints.inviteById(inviteId),
      body: model.toJson(),
    );
    return InviteDetailsModel.fromJson(json);
  }

  Future<InviteDetailsModel> respondRsvp(
    int inviteId,
    RsvpResponseModel model,
  ) async {
    final json = await _apiClient.putJson(
      ApiEndpoints.inviteRsvp(inviteId),
      body: model.toJson(),
    );
    return InviteDetailsModel.fromJson(json);
  }

  Future<SendInviteResultModel> sendInvite(
    int inviteId, {
    String? mensagemOpcional,
  }) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.inviteSend(inviteId),
      body: {
        'mensagemOpcional': mensagemOpcional,
      }..removeWhere((key, value) => value == null),
    );
    return SendInviteResultModel.fromJson(json);
  }
}
