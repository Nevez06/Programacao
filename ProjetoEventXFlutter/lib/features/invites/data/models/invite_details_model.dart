import 'package:projeto_eventx_flutter/features/invites/data/models/invite_model.dart';

class InviteDetailsModel extends InviteModel {
  const InviteDetailsModel({
    required super.id,
    required super.eventoId,
    required super.convidadoId,
    required super.nomeConvidado,
    required super.emailConvidado,
    required super.titulo,
    required super.status,
    required super.eventoNome,
    required super.dataConvite,
    required super.checkInRealizado,
    super.codigoQr,
    this.dataEvento,
    this.horaInicio,
    this.horaFim,
    this.localNome,
    this.localEndereco,
    this.templateId,
    this.templateNome,
  });

  final DateTime? dataEvento;
  final String? horaInicio;
  final String? horaFim;
  final String? localNome;
  final String? localEndereco;
  final int? templateId;
  final String? templateNome;

  factory InviteDetailsModel.fromJson(Map<String, dynamic> json) {
    return InviteDetailsModel(
      id: InviteModel.readInt(json, 'id'),
      eventoId: InviteModel.readInt(json, 'eventoId'),
      convidadoId: InviteModel.readInt(json, 'convidadoId'),
      nomeConvidado: InviteModel.readString(json, 'nomeConvidado'),
      emailConvidado: InviteModel.readString(json, 'emailConvidado'),
      titulo: InviteModel.readString(json, 'titulo'),
      status: InviteModel.readString(json, 'status'),
      eventoNome: InviteModel.readString(json, 'eventoNome'),
      dataConvite: InviteModel.readDateTime(json, 'dataConvite'),
      checkInRealizado: InviteModel.readBool(json, 'checkInRealizado'),
      codigoQr: InviteModel.readNullableString(json, 'codigoQr'),
      dataEvento: _readNullableDateTime(json, 'dataEvento'),
      horaInicio: InviteModel.readNullableString(json, 'horaInicio'),
      horaFim: InviteModel.readNullableString(json, 'horaFim'),
      localNome: InviteModel.readNullableString(json, 'localNome'),
      localEndereco: InviteModel.readNullableString(json, 'localEndereco'),
      templateId: _readNullableInt(json, 'templateId'),
      templateNome: InviteModel.readNullableString(json, 'templateNome'),
    );
  }

  static DateTime? _readNullableDateTime(
    Map<String, dynamic> json,
    String key,
  ) {
    final value = json[key] ?? json[InviteModel.toPascalCase(key)];
    if (value == null) {
      return null;
    }
    return DateTime.tryParse(value.toString());
  }

  static int? _readNullableInt(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[InviteModel.toPascalCase(key)];
    return (value as num?)?.toInt();
  }
}
