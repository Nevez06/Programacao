class InviteModel {
  const InviteModel({
    required this.id,
    required this.eventoId,
    required this.convidadoId,
    required this.nomeConvidado,
    required this.emailConvidado,
    required this.titulo,
    required this.status,
    required this.eventoNome,
    required this.dataConvite,
    required this.checkInRealizado,
    this.codigoQr,
  });

  final int id;
  final int eventoId;
  final int convidadoId;
  final String nomeConvidado;
  final String emailConvidado;
  final String titulo;
  final String status;
  final String eventoNome;
  final DateTime dataConvite;
  final bool checkInRealizado;
  final String? codigoQr;

  factory InviteModel.fromJson(Map<String, dynamic> json) {
    return InviteModel(
      id: readInt(json, 'id'),
      eventoId: readInt(json, 'eventoId'),
      convidadoId: readInt(json, 'convidadoId'),
      nomeConvidado: readString(json, 'nomeConvidado'),
      emailConvidado: readString(json, 'emailConvidado'),
      titulo: readString(json, 'titulo'),
      status: readString(json, 'status'),
      eventoNome: readString(json, 'eventoNome'),
      dataConvite: readDateTime(json, 'dataConvite'),
      checkInRealizado: readBool(json, 'checkInRealizado'),
      codigoQr: readNullableString(json, 'codigoQr'),
    );
  }

  static int readInt(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[toPascalCase(key)];
    return (value as num?)?.toInt() ?? 0;
  }

  static String readString(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[toPascalCase(key)];
    return (value ?? '').toString();
  }

  static String? readNullableString(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[toPascalCase(key)];
    final text = value?.toString();
    if (text == null || text.isEmpty) {
      return null;
    }
    return text;
  }

  static bool readBool(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[toPascalCase(key)];
    if (value is bool) {
      return value;
    }
    if (value is String) {
      if (value.toLowerCase() == 'true') {
        return true;
      }
      if (value.toLowerCase() == 'false') {
        return false;
      }
    }
    return false;
  }

  static DateTime readDateTime(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[toPascalCase(key)];
    return DateTime.tryParse((value ?? '').toString()) ??
        DateTime.fromMillisecondsSinceEpoch(0);
  }

  static String toPascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }
}
