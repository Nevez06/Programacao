import 'package:projeto_eventx_flutter/features/invites/data/invites_repository.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/invite_details_model.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/invite_model.dart';

class InvitationRepository {
  InvitationRepository(this._source);

  final InvitesRepository _source;

  Future<List<InviteModel>> getInvites() => _source.getInvites();
  Future<InviteDetailsModel> getInviteDetails(int inviteId) =>
      _source.getInviteDetails(inviteId);
}
