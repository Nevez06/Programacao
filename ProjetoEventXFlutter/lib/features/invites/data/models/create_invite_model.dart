import 'package:projeto_eventx_flutter/features/invites/data/models/invite_details_model.dart';

class CreateInviteModel {
  const CreateInviteModel({
    required this.eventoId,
    this.convidadoId,
    this.nomeConvidado,
    this.emailConvidado,
    this.templateId,
    this.enviarAgora = false,
    this.mensagemOpcional,
  });

  final int eventoId;
  final int? convidadoId;
  final String? nomeConvidado;
  final String? emailConvidado;
  final int? templateId;
  final bool enviarAgora;
  final String? mensagemOpcional;

  Map<String, dynamic> toJson() {
    return {
      'eventoId': eventoId,
      'convidadoId': convidadoId,
      'nomeConvidado': nomeConvidado,
      'emailConvidado': emailConvidado,
      'templateId': templateId,
      'enviarAgora': enviarAgora,
      'mensagemOpcional': mensagemOpcional,
    }..removeWhere((key, value) => value == null);
  }
}

class CreateInviteResultModel {
  const CreateInviteResultModel({
    required this.invite,
    this.emailSent,
  });

  final InviteDetailsModel invite;
  final bool? emailSent;

  factory CreateInviteResultModel.fromJson(Map<String, dynamic> json) {
    final inviteJson = (json['invite'] as Map<String, dynamic>?) ??
        (json['Invite'] as Map<String, dynamic>?) ??
        <String, dynamic>{};
    return CreateInviteResultModel(
      invite: InviteDetailsModel.fromJson(inviteJson),
      emailSent: (json['emailSent'] as bool?) ?? (json['EmailSent'] as bool?),
    );
  }
}
